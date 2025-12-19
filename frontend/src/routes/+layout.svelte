<script lang="ts">
	import "../app.css";
	import { user, logout } from "$lib/auth";
	import { page } from "$app/stores";
	import { onMount } from "svelte";
	import { goto } from "$app/navigation";
	import { LogOut, CircleHelp, Ship } from "lucide-svelte";
	let { children } = $props();

	import { headerState } from "$lib/header.svelte";
	import { settings } from "$lib/settings.svelte";
	import SettingsMenu from "$lib/components/SettingsMenu.svelte";

	$effect(() => {
		// Protect routes, redirect to login if no user
		if (!$user && $page.url.pathname !== "/login") {
			goto("/login");
		}
	});

	onMount(() => {
		settings.init();
	});

	// Mock initial user for dev if needed, or rely on localStorage
</script>

<svelte:head>
	<title>Model Manager</title>
</svelte:head>

<div
	class="min-h-screen bg-gray-50 dark:bg-slate-900 font-sans antialiased transition-colors duration-200"
>
	{#if $user}
		<header
			class="bg-white dark:bg-slate-800 border-b border-gray-200 dark:border-slate-700 px-4 py-3 sticky top-0 z-40 transition-colors duration-200"
		>
			<div class="container mx-auto flex justify-between items-center">
				<a
					href="/"
					class="font-bold text-xl bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent flex items-center gap-2"
				>
					<Ship size={24} class="text-blue-600" />
					Port Model Manager
				</a>
				<div class="flex items-center gap-2">
					{@render headerState.actions?.()}

					<div
						class="h-6 w-px bg-gray-200 dark:bg-slate-700 mx-2"
					></div>

					<span class="text-sm text-gray-600 dark:text-gray-400">
						Hello, <span
							class="font-semibold text-gray-900 dark:text-gray-200"
							>{$user.username}</span
						>
					</span>

					<div
						class="h-6 w-px bg-gray-200 dark:bg-slate-700 mx-2"
					></div>

					<SettingsMenu />

					<a
						href="/help"
						class="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
						title="User Manual"
					>
						<CircleHelp size={18} />
					</a>
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
	<div class="dark:text-gray-100">
		{@render children()}
	</div>
</div>
