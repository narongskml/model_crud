<script lang="ts">
    import { api } from "$lib/api";
    import type { PortModelMapping } from "$lib/types";
    import { goto } from "$app/navigation";
    import { page } from "$app/stores";
    import { user } from "$lib/auth";
    import { onMount } from "svelte";
    import { Loader2, ArrowLeft, Save, AlertCircle } from "lucide-svelte";

    let form = $state<PortModelMapping>({
        accnoSleeve: "",
        effectiveDate: "",
        modelName: "",
        currencyModel: "",
        hedgeModelName: "",
        isDeleted: false,
    });

    let loading = $state(true);
    let saving = $state(false);
    let error = $state<string | null>(null);
    let warnings = $state<string[]>([]);

    let accno = $derived($page.params.accno ?? "");
    let date = $derived($page.params.date ?? "");

    async function loadRecord() {
        loading = true;

        // Role Check
        if (!$user?.roles?.includes("model-manager")) {
            alert("Unauthorized: You do not have permission to edit records.");
            goto("/");
            return;
        }

        try {
            const data = await api.getMapping(accno, date);
            form = data;
        } catch (err: any) {
            error = err.message;
        } finally {
            loading = false;
        }
    }

    onMount(() => {
        loadRecord();
    });

    async function handleSubmit(e: Event) {
        e.preventDefault();
        saving = true;
        error = null;
        warnings = [];

        try {
            const res = await api.updateMapping(accno, date, form);
            if (res && res.warnings && res.warnings.length > 0) {
                warnings = res.warnings;
                setTimeout(() => goto("/"), 2000);
            } else {
                goto("/");
            }
        } catch (err: any) {
            error = err.message;
        } finally {
            saving = false;
        }
    }
</script>

<svelte:head>
    <title>Edit - Model Manager</title>
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
            Edit Model Mapping
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
            Update existing portfolio assignment.
        </p>
    </div>

    {#if loading}
        <div class="flex justify-center py-12">
            <Loader2 class="animate-spin text-blue-600" size={40} />
        </div>
    {:else if error}
        <div
            class="bg-red-50 border border-red-200 text-red-700 p-4 rounded-lg mb-6 flex items-start gap-3"
        >
            <AlertCircle class="mt-0.5" size={20} />
            <div>
                <p class="font-bold">Error loading record</p>
                <p>{error}</p>
                <button
                    onclick={loadRecord}
                    class="mt-2 text-sm underline hover:text-red-800"
                    >Retry</button
                >
            </div>
        </div>
    {:else}
        {#if warnings.length > 0}
            <div
                class="bg-yellow-50 border border-yellow-200 text-yellow-800 p-4 rounded-lg mb-6"
            >
                <p class="font-bold">Update successful with warnings:</p>
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
                <!-- Primary Keys are Read-Only -->
                <div class="space-y-2">
                    <label
                        class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                        for="accno">Account Sleeve</label
                    >
                    <input
                        id="accno"
                        type="text"
                        value={form.accnoSleeve}
                        disabled
                        class="w-full px-4 py-2 rounded-lg border border-gray-200 dark:border-slate-600 bg-gray-50 dark:bg-slate-700 text-gray-500 dark:text-gray-400 cursor-not-allowed"
                    />
                </div>

                <div class="space-y-2">
                    <label
                        class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                        for="date">Effective Date</label
                    >
                    <input
                        id="date"
                        type="date"
                        value={form.effectiveDate}
                        disabled
                        class="w-full px-4 py-2 rounded-lg border border-gray-200 dark:border-slate-600 bg-gray-50 dark:bg-slate-700 text-gray-500 dark:text-gray-400 cursor-not-allowed"
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
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-slate-600 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all bg-white dark:bg-slate-700 text-gray-900 dark:text-gray-100"
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
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-slate-600 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all bg-white dark:bg-slate-700 text-gray-900 dark:text-gray-100"
                    >
                        <option value="A" class="bg-white dark:bg-slate-800"
                            >A - Asset Model</option
                        >
                        <option value="M" class="bg-white dark:bg-slate-800"
                            >M - Security Model</option
                        >
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
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                    />
                </div>

                <!-- Metadata Read-Only -->
                <div
                    class="space-y-2 md:col-span-2 pt-4 border-t border-gray-100 dark:border-slate-700 grid grid-cols-2 gap-4 text-xs text-gray-400 dark:text-gray-500"
                >
                    <div>
                        <p>Created by: {form.createdBy || "-"}</p>
                        <p>
                            At: {form.createdAt
                                ? new Date(form.createdAt).toLocaleString()
                                : "-"}
                        </p>
                    </div>
                    <div>
                        <p>Updated by: {form.updatedBy || "-"}</p>
                        <p>
                            At: {form.updatedAt
                                ? new Date(form.updatedAt).toLocaleString()
                                : "-"}
                        </p>
                    </div>
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
                    disabled={saving}
                    class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium shadow-md hover:shadow-lg transition-all flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
                >
                    {#if saving}
                        <Loader2 class="animate-spin" size={20} />
                        Saving...
                    {:else}
                        <Save size={20} />
                        Update Record
                    {/if}
                </button>
            </div>
        </form>
    {/if}
</div>
