<script lang="ts">
    import { api } from "$lib/api";
    import type { PortModelMappingAudit } from "$lib/types";
    import { X, Loader2, Calendar, User, Activity } from "lucide-svelte";
    import { onMount } from "svelte";

    let { accno, date, isOpen = $bindable() } = $props();

    let history = $state<PortModelMappingAudit[]>([]);
    let loading = $state(true);

    async function loadHistory() {
        loading = true;
        try {
            history = await api.getAuditHistory(accno, date);
        } catch (err) {
            console.error(err);
        } finally {
            loading = false;
        }
    }

    $effect(() => {
        if (isOpen) {
            loadHistory();
        }
    });

    function close() {
        isOpen = false;
    }

    function getActionColor(action: string) {
        switch (action) {
            case "I":
                return "bg-green-100 text-green-700";
            case "U":
                return "bg-blue-100 text-blue-700";
            case "D":
                return "bg-red-100 text-red-700";
            default:
                return "bg-gray-100 text-gray-700";
        }
    }

    function getActionLabel(action: string) {
        switch (action) {
            case "I":
                return "Created";
            case "U":
                return "Updated";
            case "D":
                return "Deleted";
            default:
                return action;
        }
    }
</script>

{#if isOpen}
    <div
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
        role="dialog"
    >
        <div
            class="bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[80vh] flex flex-col"
        >
            <div
                class="p-6 border-b border-gray-100 flex justify-between items-center bg-gray-50/50 rounded-t-xl"
            >
                <div>
                    <h2 class="text-xl font-bold text-gray-800">
                        Audit History
                    </h2>
                    <p class="text-sm text-gray-500 mt-1 flex gap-2">
                        <span
                            class="font-mono bg-gray-100 px-2 rounded text-gray-600"
                            >{accno}</span
                        >
                        <span>•</span>
                        <span>{date}</span>
                    </p>
                </div>
                <button
                    onclick={close}
                    class="p-2 hover:bg-gray-200 rounded-full transition-colors text-gray-500"
                >
                    <X size={20} />
                </button>
            </div>

            <div class="overflow-y-auto p-6 flex-1">
                {#if loading}
                    <div class="flex justify-center py-12">
                        <Loader2 class="animate-spin text-blue-600" size={32} />
                    </div>
                {:else if history.length === 0}
                    <div class="text-center py-12 text-gray-400">
                        No audit history found.
                    </div>
                {:else}
                    <div
                        class="relative pl-4 border-l-2 border-gray-100 space-y-8"
                    >
                        {#each history as item}
                            <div class="relative">
                                <div
                                    class={`absolute -left-[25px] w-4 h-4 rounded-full border-2 border-white ring-1 ring-gray-200 ${item.action === "D" ? "bg-red-500" : "bg-blue-500"}`}
                                ></div>
                                <div
                                    class="bg-white border boundary border-gray-100 rounded-lg p-4 shadow-sm hover:shadow-md transition-shadow"
                                >
                                    <div
                                        class="flex justify-between items-start mb-2"
                                    >
                                        <span
                                            class={`px-2 py-0.5 rounded text-xs font-bold uppercase tracking-wider ${getActionColor(item.action)}`}
                                        >
                                            {getActionLabel(item.action)}
                                        </span>
                                        <span
                                            class="text-xs text-gray-400 flex items-center gap-1"
                                        >
                                            <Calendar size={12} />
                                            {new Date(
                                                item.changedAt,
                                            ).toLocaleString()}
                                        </span>
                                    </div>

                                    <div
                                        class="text-sm text-gray-600 mb-2 flex items-center gap-2"
                                    >
                                        <User size={14} class="text-gray-400" />
                                        <span class="font-medium text-gray-700"
                                            >{item.changedBy}</span
                                        >
                                    </div>

                                    <div class="space-y-1 text-sm">
                                        {#if item.modelName}<div
                                                class="flex gap-2"
                                            >
                                                <span class="text-gray-400 w-24"
                                                    >Model:</span
                                                >
                                                <span
                                                    class="text-gray-800 font-medium"
                                                    >{item.modelName}</span
                                                >
                                            </div>{/if}
                                        {#if item.currencyModel}<div
                                                class="flex gap-2"
                                            >
                                                <span class="text-gray-400 w-24"
                                                    >Currency:</span
                                                >
                                                <span class="text-gray-800"
                                                    >{item.currencyModel}</span
                                                >
                                            </div>{/if}
                                        {#if item.hedgeModelName}<div
                                                class="flex gap-2"
                                            >
                                                <span class="text-gray-400 w-24"
                                                    >Hedge:</span
                                                >
                                                <span class="text-gray-800"
                                                    >{item.hedgeModelName}</span
                                                >
                                            </div>{/if}
                                    </div>
                                </div>
                            </div>
                        {/each}
                    </div>
                {/if}
            </div>

            <div
                class="p-4 border-t border-gray-100 bg-gray-50/50 rounded-b-xl flex justify-end"
            >
                <button
                    onclick={close}
                    class="px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 text-gray-700 font-medium shadow-sm transition-colors"
                >
                    Close
                </button>
            </div>
        </div>
    </div>
{/if}
