import { describe, it, expect } from 'vitest';
import { api } from './client';

describe('api client', () => {
	it('exposes typed POST for prompt generation', () => {
		expect(typeof api.POST).toBe('function');
	});
});
