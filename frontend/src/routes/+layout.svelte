<script lang="ts">
	import "../app.css";
	import { user, logout } from "$lib/auth";
	import { page } from "$app/stores";
	import { onMount } from "svelte";
	import { goto } from "$app/navigation";
	import { LogOut } from "lucide-svelte";

	let { children } = $props();

	$effect(() => {
		// Protect routes, redirect to login if no user
		if (!$user && $page.url.pathname !== "/login") {
			goto("/login");
		}
	});

	// Mock initial user for dev if needed, or rely on localStorage
</script>

<div class="min-h-screen bg-gray-50 font-sans antialiased">
	{#if $user}
		<header
			class="bg-white border-b border-gray-200 px-4 py-3 sticky top-0 z-40"
		>
			<div class="container mx-auto flex justify-between items-center">
				<a href="/" class="font-bold text-lg text-gray-800"
					>Port Model Manager</a
				>
				<div class="flex items-center gap-4">
					<span class="text-sm text-gray-600">
						Hello, <span class="font-semibold text-gray-900"
							>{$user.username}</span
						>
					</span>
					<button
						onclick={logout}
						class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
						title="Logout"
					>
						<LogOut size={18} />
					</button>
				</div>
			</div>
		</header>
	{/if}
	{@render children()}
</div>
