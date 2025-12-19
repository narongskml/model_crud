<script lang="ts">
    import { api } from "$lib/api";
    import { token, user } from "$lib/auth";
    import { goto } from "$app/navigation";
    import { Loader2, LogIn } from "lucide-svelte";
    import { settings } from "$lib/settings.svelte";
    import SettingsMenu from "$lib/components/SettingsMenu.svelte";
    import { onMount } from "svelte";

    let username = $state("");
    let password = $state("");
    let loading = $state(false);
    let error = $state<string | null>(null);

    onMount(() => {
        settings.init();
    });

    async function handleLogin(e: Event) {
        e.preventDefault();
        loading = true;
        error = null;

        try {
            const res = await api.login(username, password);
            token.set(res.token);
            user.set({ username: res.username, roles: res.roles });
            console.log(`User ${res.username} logged in successfully`);
            goto("/");
        } catch (err: any) {
            console.warn(`Login failed for user ${username}: ${err.message}`);
            error = err.message;
        } finally {
            loading = false;
        }
    }
</script>

<svelte:head>
    <title>Sign In - Model Manager</title>
</svelte:head>

<div
    class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-slate-900 px-4 transition-colors duration-200 relative"
>
    <!-- Theme Toggle in Top Right -->
    <div class="absolute top-4 right-4">
        <SettingsMenu />
    </div>

    <div
        class="max-w-md w-full bg-white dark:bg-slate-800 rounded-xl shadow-lg border border-gray-100 dark:border-slate-700 p-8 transition-colors duration-200"
    >
        <div class="text-center mb-8">
            <h1 class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                Sign In
            </h1>
            <p class="text-gray-500 dark:text-gray-400 mt-2">
                Access Port Model Manager
            </p>
        </div>

        {#if error}
            <div
                class="bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 p-3 rounded-lg mb-4 text-sm text-center border border-red-200 dark:border-red-800"
            >
                {error}
            </div>
        {/if}

        <form onsubmit={handleLogin} class="space-y-6">
            <div>
                <label
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                    for="username">Username</label
                >
                <input
                    id="username"
                    type="text"
                    required
                    bind:value={username}
                    class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all"
                    placeholder="Enter any username"
                />
            </div>

            <div>
                <label
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                    for="password">Password</label
                >
                <input
                    id="password"
                    type="password"
                    required
                    bind:value={password}
                    class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all"
                    placeholder="Enter password"
                />
            </div>

            <button
                type="submit"
                disabled={loading}
                class="w-full py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium transition-colors flex justify-center items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
            >
                {#if loading}
                    <Loader2 class="animate-spin" size={20} />
                    Signing in...
                {:else}
                    <LogIn size={20} />
                    Sign In
                {/if}
            </button>
        </form>
    </div>
</div>
