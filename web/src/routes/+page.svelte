<script lang="ts">
	import { api } from '#lib/api/client.ts';

	let goal = $state('');
	let audience = $state('');
	let tone = $state('');
	let result = $state('');
	let error = $state('');
	let loading = $state(false);

	async function generate(event: SubmitEvent) {
		event.preventDefault();
		loading = true;
		error = '';
		result = '';

		const { data, error: err } = await api.POST('/api/prompts/generate', {
			body: { goal, audience: audience || null, tone: tone || null }
		});

		loading = false;
		if (err || !data) {
			error = 'Could not generate a prompt. Check your input and try again.';
			return;
		}
		result = data.prompt;
	}
</script>

<main>
	<h1>Prompt Magic</h1>

	<form onsubmit={generate}>
		<label>
			Goal
			<textarea
				bind:value={goal}
				required
				minlength="3"
				rows="3"
				placeholder="What do you want the AI to do?"></textarea>
		</label>
		<label>
			Audience <input bind:value={audience} placeholder="Optional" />
		</label>
		<label>
			Tone <input bind:value={tone} placeholder="Optional" />
		</label>
		<button disabled={loading}>{loading ? 'Generating…' : 'Generate prompt'}</button>
	</form>

	{#if error}<p role="alert" class="error">{error}</p>{/if}
	{#if result}<pre data-testid="result">{result}</pre>{/if}
</main>

<style>
	main {
		max-width: 40rem;
		margin: 2rem auto;
		padding: 0 1rem;
		font-family: system-ui, sans-serif;
	}
	form {
		display: grid;
		gap: 0.75rem;
	}
	label {
		display: grid;
		gap: 0.25rem;
		font-weight: 600;
	}
	textarea,
	input {
		font: inherit;
		padding: 0.5rem;
	}
	pre {
		white-space: pre-wrap;
		background: #f4f4f5;
		color: #18181b;
		padding: 1rem;
		border-radius: 0.5rem;
	}
	.error {
		color: #b91c1c;
	}
</style>
