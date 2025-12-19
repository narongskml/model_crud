<script lang="ts">
    import { settings } from "$lib/settings.svelte";
    import { Settings, Moon, Sun, Type, Monitor } from "lucide-svelte";
    import { fly } from "svelte/transition";

    let isOpen = $state(false);

    function toggleOpen() {
        isOpen = !isOpen;
    }

    function close() {
        isOpen = false;
    }

    // Click outside handler logic would go here or use a library,
    // for simplicity we'll just close on selection.
</script>

<div class="relative">
    <button
        onclick={toggleOpen}
        class="p-2 text-gray-400 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
        title="Settings"
    >
        <Settings size={18} />
    </button>

    {#if isOpen}
        <!-- Backdrop -->
        <div
            class="fixed inset-0 z-40"
            onclick={close}
            role="button"
            tabindex="-1"
            onkeydown={(e) => e.key === "Escape" && close()}
        ></div>

        <div
            transition:fly={{ y: 10, duration: 200 }}
            class="absolute right-0 mt-2 w-64 bg-white dark:bg-slate-800 rounded-xl shadow-xl border border-gray-100 dark:border-slate-700 p-4 z-50"
        >
            <h3
                class="text-sm font-semibold text-gray-900 dark:text-gray-100 mb-3"
            >
                Appearance
            </h3>

            <!-- Theme Toggle -->
            <div class="mb-4">
                <p
                    class="text-xs text-gray-500 dark:text-gray-400 mb-2 font-medium"
                >
                    Theme
                </p>
                <div class="flex bg-gray-100 dark:bg-gray-700 rounded-lg p-1">
                    <button
                        onclick={() => settings.setTheme("light")}
                        class="flex-1 flex items-center justify-center gap-2 py-1.5 text-sm rounded-md transition-all {settings.theme ===
                        'light'
                            ? 'bg-white dark:bg-gray-600 text-blue-600 shadow-sm'
                            : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'}"
                    >
                        <Sun size={14} />
                        Light
                    </button>
                    <button
                        onclick={() => settings.setTheme("dark")}
                        class="flex-1 flex items-center justify-center gap-2 py-1.5 text-sm rounded-md transition-all {settings.theme ===
                        'dark'
                            ? 'bg-white dark:bg-gray-600 text-blue-600 shadow-sm'
                            : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'}"
                    >
                        <Moon size={14} />
                        Dark
                    </button>
                </div>
            </div>

            <!-- Font Size Toggle -->
            <div>
                <p
                    class="text-xs text-gray-500 dark:text-gray-400 mb-2 font-medium"
                >
                    Font Size
                </p>
                <div class="flex bg-gray-100 dark:bg-gray-700 rounded-lg p-1">
                    <button
                        onclick={() => settings.setFontSize("small")}
                        class="flex-1 py-1.5 text-xs rounded-md transition-all {settings.fontSize ===
                        'small'
                            ? 'bg-white dark:bg-gray-600 text-blue-600 shadow-sm'
                            : 'text-gray-500 dark:text-gray-400'}"
                    >
                        Aa
                    </button>
                    <button
                        onclick={() => settings.setFontSize("medium")}
                        class="flex-1 py-1.5 text-sm rounded-md transition-all {settings.fontSize ===
                        'medium'
                            ? 'bg-white dark:bg-gray-600 text-blue-600 shadow-sm'
                            : 'text-gray-500 dark:text-gray-400'}"
                    >
                        Aa
                    </button>
                    <button
                        onclick={() => settings.setFontSize("large")}
                        class="flex-1 py-1.5 text-base rounded-md transition-all {settings.fontSize ===
                        'large'
                            ? 'bg-white dark:bg-gray-600 text-blue-600 shadow-sm'
                            : 'text-gray-500 dark:text-gray-400'}"
                    >
                        Aa
                    </button>
                </div>
            </div>
        </div>
    {/if}
</div>
