<script lang="ts">
    import { api } from "$lib/api";
    import type { PortModelMapping } from "$lib/types";
    import { goto } from "$app/navigation";
    import { Loader2, ArrowLeft, Save, AlertCircle } from "lucide-svelte";
    import { onMount } from "svelte";

    let form = $state<PortModelMapping>({
        accnoSleeve: "",
        effectiveDate: new Date().toISOString().split("T")[0],
        modelName: "",
        currencyModel: "A",
        hedgeModelName: "",
        isDeleted: false,
    });

    import { user } from "$lib/auth";

    let portfolios = $state<{ code: string; name: string }[]>([]);
    let loading = $state(false);
    let error = $state<string | null>(null);
    let warnings = $state<string[]>([]);

    onMount(async () => {
        // Role Check
        if (!$user?.roles?.includes("model-manager")) {
            alert(
                "Unauthorized: You do not have permission to create records.",
            );
            goto("/");
            return;
        }

        try {
            portfolios = await api.getPortfolios();
            // Default select first if available
            if (portfolios.length > 0) {
                form.accnoSleeve = portfolios[0].code;
            }
        } catch (err) {
            console.error("Failed to load portfolios", err);
        }
    });

    async function handleSubmit(e: Event) {
        e.preventDefault();
        loading = true;
        error = null;
        warnings = [];

        try {
            const res = await api.createMapping(form);
            if (res.warnings && res.warnings.length > 0) {
                warnings = res.warnings;
                setTimeout(() => goto("/"), 2000);
            } else {
                goto("/");
            }
        } catch (err: any) {
            error = err.message;
        } finally {
            loading = false;
        }
    }
</script>

<svelte:head>
    <title>Create New - Model Manager</title>
</svelte:head>

<div class="container mx-auto px-4 py-8 max-w-2xl">
    <div class="mb-8">
        <a
            href="/"
            class="text-gray-500 hover:text-gray-800 flex items-center gap-2 mb-4 transition-colors"
        >
            <ArrowLeft size={20} />
            Back to Dashboard
        </a>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Create New Model Mapping
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
            Define a new portfolio model assignment.
        </p>
    </div>

    {#if error}
        <div
            class="bg-red-50 border border-red-200 text-red-700 p-4 rounded-lg mb-6 flex items-start gap-3 animate-pulse"
        >
            <AlertCircle class="mt-0.5" size={20} />
            <div>
                <p class="font-bold">Error creating record</p>
                <p>{error}</p>
            </div>
        </div>
    {/if}

    {#if warnings.length > 0}
        <div
            class="bg-yellow-50 border border-yellow-200 text-yellow-800 p-4 rounded-lg mb-6"
        >
            <p class="font-bold">Success with warnings:</p>
            <ul class="list-disc pl-5 mt-1">
                {#each warnings as w}
                    <li>{w}</li>
                {/each}
            </ul>
            <p class="text-sm mt-2 text-yellow-700">Redirecting...</p>
        </div>
    {/if}

    <form
        onsubmit={handleSubmit}
        class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-gray-100 dark:border-slate-700 p-8 space-y-6 transition-colors duration-200"
    >
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
                <label
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                    for="accno">Account Sleeve</label
                >
                {#if portfolios.length > 0}
                    <select
                        id="accno"
                        required
                        bind:value={form.accnoSleeve}
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-slate-600 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all bg-white dark:bg-slate-700 text-gray-900 dark:text-gray-100"
                    >
                        {#each portfolios as p}
                            <option value={p.code}>{p.code} - {p.name}</option>
                        {/each}
                    </select>
                {:else}
                    <input
                        id="accno"
                        type="text"
                        required
                        maxlength="20"
                        bind:value={form.accnoSleeve}
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-slate-600 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all bg-white dark:bg-slate-700 text-gray-900 dark:text-gray-100"
                        placeholder="e.g. ACC123"
                    />
                {/if}
            </div>

            <div class="space-y-2">
                <label
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                    for="date">Effective Date</label
                >
                <input
                    id="date"
                    type="date"
                    required
                    bind:value={form.effectiveDate}
                    class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all"
                />
            </div>

            <div class="space-y-2 md:col-span-2">
                <label
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                    for="model">Model Name</label
                >
                <input
                    id="model"
                    type="text"
                    required
                    maxlength="50"
                    bind:value={form.modelName}
                    class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all"
                    placeholder="e.g. GROWTH_STRATEGY"
                />
            </div>

            <div class="space-y-2">
                <label
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                    for="currency">Currency Model</label
                >
                <select
                    id="currency"
                    bind:value={form.currencyModel}
                    class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all bg-white"
                >
                    <option value="A">A - Asset Model</option>
                    <option value="M">M - Security Model</option>
                </select>
            </div>

            <div class="space-y-2">
                <label
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                    for="hedge">Hedge Model</label
                >
                <input
                    id="hedge"
                    type="text"
                    maxlength="50"
                    bind:value={form.hedgeModelName}
                    class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all"
                    placeholder="Optional"
                />
            </div>
        </div>

        <div class="pt-4 flex justify-end gap-3">
            <a
                href="/"
                class="px-6 py-2 border border-gray-300 dark:border-slate-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-slate-700 font-medium transition-colors"
            >
                Cancel
            </a>
            <button
                type="submit"
                disabled={loading}
                class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium shadow-md hover:shadow-lg transition-all flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
            >
                {#if loading}
                    <Loader2 class="animate-spin" size={20} />
                    Saving...
                {:else}
                    <Save size={20} />
                    Create Record
                {/if}
            </button>
        </div>
    </form>
</div>
