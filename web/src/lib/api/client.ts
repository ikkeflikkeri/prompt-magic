import createClient from 'openapi-fetch';
import type { paths } from './schema';

// Same-origin in production (ASP.NET serves the SPA); proxied to the API by Vite in dev.
export const api = createClient<paths>({ baseUrl: '' });
