import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [sveltekit()],
	server: {
		port: 5173,
		proxy: {
			'/api': {
				target: 'http://localhost:5137',
				changeOrigin: true,
				secure: false,
				rewrite: (path) => path.replace(/^\/api/, '/api')
			}
		}
	}
});
