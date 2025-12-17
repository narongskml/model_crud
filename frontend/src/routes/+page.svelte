<script lang="ts">
    import { onMount } from "svelte";
    import { api } from "$lib/api";
    import type { PortModelMapping } from "$lib/types";
    import {
        Plus,
        Pencil,
        Trash2,
        History,
        Loader2,
        AlertCircle,
    } from "lucide-svelte";
    import AuditLogModal from "$lib/components/AuditLogModal.svelte";

    let mappings = $state<PortModelMapping[]>([]);
    let loading = $state(true);
    let error = $state<string | null>(null);
    let searchQuery = $state("");

    // Audit Modal State
    let showAudit = $state(false);
    let auditRecord = $state<{ accno: string; date: string } | null>(null);

    async function loadData() {
        loading = true;
        try {
            mappings = await api.getMappings();
            error = null;
        } catch (err: any) {
            error = err.message;
        } finally {
            loading = false;
        }
    }

    onMount(() => {
        loadData();
    });

    async function handleDelete(accno: string, date: string) {
        if (!confirm("Are you sure you want to delete this record?")) return;
        try {
            await api.deleteMapping(accno, date);
            mappings = mappings.filter(
                (m) => !(m.accnoSleeve === accno && m.effectiveDate === date),
            );
        } catch (err: any) {
            alert(err.message);
        }
    }

    function openAudit(accno: string, date: string) {
        auditRecord = { accno, date };
        showAudit = true;
    }

    // Derived state for filtering
    let filteredMappings = $derived(
        mappings.filter(
            (m) =>
                m.accnoSleeve
                    .toLowerCase()
                    .includes(searchQuery.toLowerCase()) ||
                m.modelName.toLowerCase().includes(searchQuery.toLowerCase()),
        ),
    );
</script>

<div class="container mx-auto px-4 py-8">
    <div class="flex justify-between items-center mb-6">
        <h1
            class="text-3xl font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent"
        >
            Port Model Manager
        </h1>
        <a
            href="/create"
            class="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors shadow-lg hover:shadow-xl"
        >
            <Plus size={20} />
            Create New
        </a>
    </div>

    <!-- Search Tool -->
    <div class="mb-6 relative">
        <input
            type="text"
            placeholder="Search mappings..."
            bind:value={searchQuery}
            class="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all pl-4"
        />
    </div>

    {#if loading}
        <div class="flex justify-center py-12">
            <Loader2 class="animate-spin text-blue-600" size={40} />
        </div>
    {:else if error}
        <div
            class="bg-red-50 text-red-600 p-4 rounded-lg flex items-center gap-2"
        >
            <AlertCircle size={20} />
            {error}
        </div>
    {:else}
        <div
            class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden"
        >
            <div class="overflow-x-auto">
                <table class="w-full text-left">
                    <thead class="bg-gray-50 border-b border-gray-100">
                        <tr>
                            <th class="px-6 py-4 font-semibold text-gray-600"
                                >Account Sleeve</th
                            >
                            <th class="px-6 py-4 font-semibold text-gray-600"
                                >Effective Date</th
                            >
                            <th class="px-6 py-4 font-semibold text-gray-600"
                                >Model Name</th
                            >
                            <th class="px-6 py-4 font-semibold text-gray-600"
                                >Currency</th
                            >
                            <th class="px-6 py-4 font-semibold text-gray-600"
                                >Hedge Model</th
                            >
                            <th
                                class="px-6 py-4 font-semibold text-gray-600 text-right"
                                >Actions</th
                            >
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-50">
                        {#each filteredMappings as item}
                            <tr class="hover:bg-gray-50/50 transition-colors">
                                <td class="px-6 py-4 font-medium text-gray-800"
                                    >{item.accnoSleeve}</td
                                >
                                <td class="px-6 py-4 text-gray-600"
                                    >{item.effectiveDate}</td
                                >
                                <td class="px-6 py-4">
                                    <span
                                        class="px-3 py-1 bg-blue-50 text-blue-700 rounded-full text-sm font-medium"
                                    >
                                        {item.modelName}
                                    </span>
                                </td>
                                <td class="px-6 py-4 text-gray-600"
                                    >{item.currencyModel || "-"}</td
                                >
                                <td class="px-6 py-4 text-gray-600"
                                    >{item.hedgeModelName || "-"}</td
                                >
                                <td class="px-6 py-4 text-right">
                                    <div class="flex justify-end gap-2">
                                        <button
                                            onclick={() =>
                                                openAudit(
                                                    item.accnoSleeve,
                                                    item.effectiveDate,
                                                )}
                                            class="p-2 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                                            title="View Audit Log"
                                        >
                                            <History size={18} />
                                        </button>
                                        <a
                                            href={`/edit/${item.accnoSleeve}/${item.effectiveDate}`}
                                            class="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                                            title="Edit"
                                        >
                                            <Pencil size={18} />
                                        </a>
                                        <button
                                            onclick={() =>
                                                handleDelete(
                                                    item.accnoSleeve,
                                                    item.effectiveDate,
                                                )}
                                            class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                            title="Delete"
                                        >
                                            <Trash2 size={18} />
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        {/each}
                        {#if filteredMappings.length === 0}
                            <tr>
                                <td
                                    colspan="6"
                                    class="px-6 py-12 text-center text-gray-400"
                                >
                                    No records found.
                                </td>
                            </tr>
                        {/if}
                    </tbody>
                </table>
            </div>
        </div>
    {/if}

    {#if showAudit && auditRecord}
        <AuditLogModal
            accno={auditRecord.accno}
            date={auditRecord.date}
            bind:isOpen={showAudit}
        />
    {/if}
</div>
