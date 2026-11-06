// Automated review: reads a PR diff against the repo's standards and its ticket, posts one advisory comment.
// Dependency-free (Node 24). Inputs come from the environment, see .github/workflows/review.yml.
import { execFileSync } from 'node:child_process';
import { existsSync, readdirSync, readFileSync } from 'node:fs';

const MARKER = '<!-- automated-review -->';
const MAX_DIFF_CHARS = 60_000;
const MAX_DOC_CHARS = 20_000;

const { PR_NUMBER, GITHUB_REPOSITORY, LLM_API_KEY, LLM_MODEL, FORCE } = process.env;
const baseUrl = (process.env.LLM_BASE_URL || 'https://api.openai.com/v1').replace(/\/+$/, '');

const gh = (...args) => execFileSync('gh', args, { encoding: 'utf8', maxBuffer: 64 * 1024 * 1024 });
const ghJson = (...args) => JSON.parse(gh(...args));
const truncate = (text, max) => (text.length > max ? `${text.slice(0, max)}\n[truncated]` : text);

function fail(message) {
  console.error(`::error::${message}`);
  process.exit(1);
}

if (!PR_NUMBER || !GITHUB_REPOSITORY) fail('PR_NUMBER and GITHUB_REPOSITORY are required.');

// Not configured yet: do nothing, loudly but without failing the PR.
if (!LLM_API_KEY || !LLM_MODEL) {
  console.log('::notice::Automated review skipped: set secret REVIEW_LLM_API_KEY and variable REVIEW_LLM_MODEL.');
  process.exit(0);
}

// Dedup: one review comment per PR. A forced run (the "review" label) updates it in place.
const comments = ghJson('api', '--paginate', '--slurp', `repos/${GITHUB_REPOSITORY}/issues/${PR_NUMBER}/comments`).flat();
const existing = comments.find((c) => c.body?.includes(MARKER));
if (existing && FORCE !== 'true') {
  console.log('Already reviewed; add the "review" label to review again.');
  process.exit(0);
}

async function post(body) {
  const text = `${MARKER}\n${body}`;
  if (existing) {
    gh('api', '--method', 'PATCH', `repos/${GITHUB_REPOSITORY}/issues/comments/${existing.id}`, '-f', `body=${text}`);
  } else {
    gh('api', '--method', 'POST', `repos/${GITHUB_REPOSITORY}/issues/${PR_NUMBER}/comments`, '-f', `body=${text}`);
  }
}

const pr = ghJson('pr', 'view', PR_NUMBER, '--json', 'title,body,closingIssuesReferences');
const diff = gh('pr', 'diff', PR_NUMBER);

if (diff.length > MAX_DIFF_CHARS) {
  await post(`### Automated review\n\nSkipped: the diff is too large for an automated review (${diff.length} characters, limit ${MAX_DIFF_CHARS}). This PR needs a human.`);
  process.exit(0);
}

// The ticket: the issues this PR closes. No linked ticket means the spec axis is skipped.
const tickets = pr.closingIssuesReferences.map((ref) => {
  const issue = ghJson('issue', 'view', String(ref.number), '--json', 'number,title,body');
  return `#${issue.number} ${issue.title}\n${truncate(issue.body ?? '', MAX_DOC_CHARS)}`;
});

// The standards. Missing files are fine.
const docFiles = ['CLAUDE.md', 'CONTRIBUTING.md', 'GLOSSARY.md'];
if (existsSync('docs/adr')) {
  docFiles.push(...readdirSync('docs/adr').filter((f) => f.endsWith('.md')).sort().map((f) => `docs/adr/${f}`));
}
const standards = docFiles
  .filter((f) => existsSync(f))
  .map((f) => `--- ${f} ---\n${truncate(readFileSync(f, 'utf8'), MAX_DOC_CHARS)}`)
  .join('\n\n');

const systemPrompt = readFileSync(new URL('./prompt.md', import.meta.url), 'utf8');
const userPrompt = [
  `# Standards\n\n${standards || '(none found)'}`,
  `# Ticket\n\n${tickets.length ? tickets.join('\n\n') : '(no linked ticket)'}`,
  `# Pull request\n\nTitle: ${pr.title}\n\n${truncate(pr.body ?? '', MAX_DOC_CHARS)}`,
  `# Diff\n\n${diff}`,
].join('\n\n');

const response = await fetch(`${baseUrl}/chat/completions`, {
  method: 'POST',
  headers: { 'content-type': 'application/json', authorization: `Bearer ${LLM_API_KEY}` },
  body: JSON.stringify({
    model: LLM_MODEL,
    messages: [
      { role: 'system', content: systemPrompt },
      { role: 'user', content: userPrompt },
    ],
  }),
  signal: AbortSignal.timeout(5 * 60_000),
});
if (!response.ok) fail(`LLM request failed: ${response.status} ${truncate(await response.text(), 500)}`);

const content = (await response.json()).choices?.[0]?.message?.content ?? '';
let review;
try {
  // Tolerate a code fence around the JSON.
  review = JSON.parse(content.replace(/^\s*```(?:json)?\s*|\s*```\s*$/g, ''));
} catch {
  fail(`The model did not return valid JSON:\n${truncate(content, 500)}`);
}

const findings = Array.isArray(review.findings) ? review.findings : [];
const verdict = review.verdict === 'fail' ? 'fail' : 'pass';
const lines = [
  '### Automated review',
  '',
  `**${verdict === 'fail' ? 'Fail' : 'Pass'}**: ${review.summary ?? ''}`,
  '',
  ...findings.map((f) => `- **${f.axis}**, \`${f.file}:${f.line}\`: ${f.problem}\n  > ${String(f.evidence ?? '').replaceAll('\n', '\n  > ')}`),
  '',
  '_Advisory only. A human reviews and merges. Add the `review` label to run this again._',
];
await post(lines.join('\n'));
console.log(`Posted review: ${verdict}, ${findings.length} finding(s).`);
