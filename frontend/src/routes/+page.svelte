<script lang="ts">
    import { onMount } from "svelte";
    import { api } from "$lib/api";
    import { user } from "$lib/auth";
    import type { PortModelMapping } from "$lib/types";
    import {
        Plus,
        Pencil,
        Trash2,
        History,
        Loader2,
        AlertCircle,
        Download,
        ChevronLeft,
        ChevronRight,
        Search,
    } from "lucide-svelte";
    import AuditLogModal from "$lib/components/AuditLogModal.svelte";
    import XLSX from "xlsx-js-style";

    let mappings = $state<PortModelMapping[]>([]);
    let loading = $state(true);
    let error = $state<string | null>(null);
    let searchQuery = $state("");

    // Pagination State
    let currentPage = $state(1);
    let pageSize = $state(10);

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

    function handleExport() {
        if (!mappings || mappings.length === 0) return;

        // 1. Filter active records
        const activeRecords = mappings.filter((m) => !m.isDeleted);
        if (activeRecords.length === 0) {
            alert("No active records to export.");
            return;
        }

        // 2. Map to display columns only
        const data = activeRecords.map((m) => ({
            "Account Sleeve": m.accnoSleeve,
            "Effective Date": m.effectiveDate,
            "Model Name": m.modelName,
            "Currency Model": m.currencyModel,
            "Hedge Model": m.hedgeModelName,
        }));

        // 3. Create Worksheet
        const ws = XLSX.utils.json_to_sheet(data);

        // 4. Set Column Widths (Fit to Content)
        const colWidths = Object.keys(data[0]).map((key) => {
            const maxLen = Math.max(
                key.length,
                ...data.map(
                    (row) =>
                        (row[key as keyof typeof row] || "").toString().length,
                ),
            );
            return { wch: maxLen + 2 }; // Add padding
        });
        ws["!cols"] = colWidths;

        // 5. Style Header Row
        // Header is typically range A1:E1. We can find the range from !ref.
        const range = XLSX.utils.decode_range(ws["!ref"]!);
        for (let C = range.s.c; C <= range.e.c; ++C) {
            const address = XLSX.utils.encode_cell({ c: C, r: 0 }); // Row 0 is header
            if (!ws[address]) continue;
            ws[address].s = {
                font: {
                    bold: true,
                    color: { rgb: "000000" },
                },
                fill: {
                    fgColor: { rgb: "E9E9E9" }, // Gray background
                },
                alignment: {
                    horizontal: "center",
                },
                border: {
                    bottom: { style: "medium", color: { rgb: "000000" } },
                },
            };
        }

        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Mappings");
        XLSX.writeFile(wb, "PortModelMappings.xlsx");
    }

    // Reset pagination when search query changes
    $effect(() => {
        searchQuery;
        currentPage = 1;
    });

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

    // Derived state for pagination
    let paginatedMappings = $derived(
        pageSize === -1
            ? filteredMappings
            : filteredMappings.slice(
                  (currentPage - 1) * pageSize,
                  currentPage * pageSize,
              ),
    );

    let totalPages = $derived(
        pageSize === -1 || filteredMappings.length === 0
            ? 1
            : Math.ceil(filteredMappings.length / pageSize),
    );

    let isManager = $derived($user?.roles?.includes("model-manager") ?? false);

    import { headerState } from "$lib/header.svelte";

    $effect(() => {
        headerState.actions = actions;
        return () => {
            headerState.actions = undefined;
        };
    });
</script>

{#snippet actions()}
    <div class="flex items-center gap-2">
        <button
            onclick={handleExport}
            class="p-2 text-gray-400 hover:text-green-600 hover:bg-green-50 rounded-lg transition-colors"
            title="Export Excel"
        >
            <Download size={20} />
        </button>
        {#if isManager}
            <a
                href="/create"
                class="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                title="Create New"
            >
                <Plus size={20} />
            </a>
        {/if}
    </div>
{/snippet}

<div class="container mx-auto px-4 pt-1 pb-8">
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
                <p class="font-bold">Error loading data</p>
                <p>{error}</p>
            </div>
        </div>
    {:else}
        <div
            class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-gray-100 dark:border-slate-700 overflow-hidden transition-colors duration-200"
        >
            <div class="p-4 border-b border-gray-100 dark:border-slate-700">
                <div class="relative">
                    <Search
                        class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
                        size={20}
                    />
                    <input
                        type="text"
                        placeholder="Search mappings..."
                        bind:value={searchQuery}
                        class="w-full pl-10 pr-4 py-2 rounded-lg border border-gray-200 dark:border-slate-600 bg-white dark:bg-slate-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all"
                    />
                </div>
            </div>

            <div class="overflow-x-auto">
                <table class="w-full">
                    <thead class="bg-gray-50/50 dark:bg-slate-700/50">
                        <tr>
                            <th
                                class="px-6 py-2 text-left text-sm font-semibold text-gray-600 dark:text-gray-400"
                                >Account Sleeve</th
                            >
                            <th
                                class="px-6 py-2 text-left text-sm font-semibold text-gray-600 dark:text-gray-400"
                                >Effective Date</th
                            >
                            <th
                                class="px-6 py-2 text-left text-sm font-semibold text-gray-600 dark:text-gray-400"
                                >Model Name</th
                            >
                            <th
                                class="px-6 py-2 text-left text-sm font-semibold text-gray-600 dark:text-gray-400"
                                >Currency</th
                            >
                            <th
                                class="px-6 py-2 text-left text-sm font-semibold text-gray-600 dark:text-gray-400"
                                >Hedge Model</th
                            >
                            <th
                                class="px-6 py-2 text-right text-sm font-semibold text-gray-600 dark:text-gray-400"
                                >Actions</th
                            >
                        </tr>
                    </thead>
                    <tbody
                        class="divide-y divide-gray-50 dark:divide-slate-700"
                    >
                        {#each paginatedMappings as item}
                            <tr
                                class="hover:bg-gray-50/50 dark:hover:bg-slate-700/50 transition-colors"
                            >
                                <td
                                    class="px-6 py-2 text-gray-900 dark:text-gray-100 font-medium"
                                    >{item.accnoSleeve}</td
                                >
                                <td
                                    class="px-6 py-2 text-gray-600 dark:text-gray-400"
                                    >{new Date(
                                        item.effectiveDate,
                                    ).toLocaleDateString()}</td
                                >
                                <td class="px-6 py-2">
                                    <span
                                        class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-300"
                                    >
                                        {item.modelName}
                                    </span>
                                </td>
                                <td
                                    class="px-6 py-2 text-gray-600 dark:text-gray-400"
                                    >{item.currencyModel === "A"
                                        ? "Asset"
                                        : "Security"}</td
                                >
                                <td
                                    class="px-6 py-2 text-gray-600 dark:text-gray-400"
                                    >{item.hedgeModelName || "-"}</td
                                >
                                <td class="px-6 py-2 text-right">
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
                                        {#if isManager}
                                            {#if isManager}
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
                                            {/if}
                                        {/if}
                                    </div>
                                </td>
                            </tr>
                        {/each}
                        {#if paginatedMappings.length === 0}
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

            <!-- Pagination Controls -->
            <div
                class="bg-gray-50 dark:bg-slate-800/50 px-6 py-4 border-t border-gray-100 dark:border-slate-700 flex flex-wrap gap-4 items-center justify-between"
            >
                <div
                    class="flex items-center gap-3 text-sm text-gray-600 dark:text-gray-400"
                >
                    <span>Rows per page:</span>
                    <select
                        bind:value={pageSize}
                        class="bg-white dark:bg-slate-700 border border-gray-200 dark:border-slate-600 rounded-lg px-2 py-1 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all cursor-pointer text-gray-900 dark:text-gray-100"
                    >
                        <option value={10}>10</option>
                        <option value={20}>20</option>
                        <option value={50}>50</option>
                        <option value={100}>100</option>
                        <option value={-1}>All</option>
                    </select>
                    <span class="ml-2">
                        Showing {filteredMappings.length > 0
                            ? (currentPage - 1) *
                                  (pageSize === -1 ? 0 : pageSize) +
                              1
                            : 0} -
                        {pageSize === -1
                            ? filteredMappings.length
                            : Math.min(
                                  currentPage * pageSize,
                                  filteredMappings.length,
                              )}
                        of {filteredMappings.length}
                    </span>
                </div>

                <div class="flex items-center gap-2">
                    <button
                        disabled={currentPage === 1}
                        onclick={() => currentPage--}
                        class="p-1 px-3 py-1.5 border border-gray-200 dark:border-slate-600 rounded-lg hover:bg-white dark:hover:bg-slate-700 hover:text-blue-600 dark:hover:text-blue-400 disabled:opacity-30 disabled:cursor-not-allowed transition-all flex items-center gap-1 text-sm font-medium text-gray-700 dark:text-gray-300"
                    >
                        <ChevronLeft size={16} />
                        Previous
                    </button>
                    <span
                        class="text-sm font-medium text-gray-700 dark:text-gray-300 min-w-[3rem] text-center"
                    >
                        Page {currentPage} of {totalPages}
                    </span>
                    <button
                        disabled={currentPage === totalPages}
                        onclick={() => currentPage++}
                        class="p-1 px-3 py-1.5 border border-gray-200 dark:border-gray-600 rounded-lg hover:bg-white dark:hover:bg-gray-700 hover:text-blue-600 dark:hover:text-blue-400 disabled:opacity-30 disabled:cursor-not-allowed transition-all flex items-center gap-1 text-sm font-medium text-gray-700 dark:text-gray-300"
                    >
                        Next
                        <ChevronRight size={16} />
                    </button>
                </div>
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
