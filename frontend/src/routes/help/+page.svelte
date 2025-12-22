<script>
    import {
        ArrowLeft,
        BookOpen,
        Shield,
        LayoutDashboard,
        Database,
        FileText,
        Download,
        Upload,
        Settings,
        Zap,
    } from "lucide-svelte";
</script>

<div class="container mx-auto px-4 py-8 max-w-4xl">
    <div class="mb-8 flex items-center justify-between">
        <div>
            <a
                href="/"
                class="text-gray-500 dark:text-slate-400 hover:text-gray-800 dark:hover:text-slate-200 flex items-center gap-2 mb-2 transition-colors"
            >
                <ArrowLeft size={20} />
                Back to Dashboard
            </a>
            <h1
                class="text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-3"
            >
                <BookOpen class="text-blue-600" />
                User Manual
            </h1>
            <p class="text-gray-500 dark:text-slate-400 mt-2">
                Complete guide to using the Port Model Manager (v1.1.0).
            </p>
        </div>
    </div>

    <div class="space-y-8">
        <!-- RBAC Section -->
        <div
            class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-gray-100 dark:border-slate-700 p-6 transition-colors"
        >
            <h2
                class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2 mb-4 pb-2 border-b border-gray-100 dark:border-slate-700"
            >
                <Shield class="text-indigo-500" size={24} />
                1. Roles & Permissions
            </h2>
            <div class="space-y-4 text-gray-600 dark:text-slate-300">
                <p>
                    The system uses Role-Based Access Control (RBAC) to manage
                    permissions:
                </p>
                <ul class="list-disc pl-5 space-y-2">
                    <li>
                        <strong class="text-gray-900 dark:text-white"
                            >Model Viewer:</strong
                        > Can view data, search, and export reports.
                    </li>
                    <li>
                        <strong class="text-gray-900 dark:text-white"
                            >Model Manager:</strong
                        > Full access, including creating, editing, and deleting
                        records.
                    </li>
                </ul>
            </div>
        </div>

        <!-- Dashboard Section -->
        <div
            class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-gray-100 dark:border-slate-700 p-6 transition-colors"
        >
            <h2
                class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2 mb-4 pb-2 border-b border-gray-100 dark:border-slate-700"
            >
                <LayoutDashboard class="text-blue-500" size={24} />
                2. Dashboard Usage
            </h2>
            <div class="space-y-4 text-gray-600 dark:text-slate-300">
                <ul class="list-disc pl-5 space-y-2">
                    <li>
                        <strong class="text-gray-900 dark:text-white"
                            >Live Search:</strong
                        >
                        Filter by <em>Account Sleeve</em> or <em>Model Name</em>
                        instantly.
                    </li>
                    <li>
                        <strong class="text-gray-900 dark:text-white"
                            >Actions:</strong
                        >
                        <ul class="list-circle pl-5 mt-1 space-y-1 text-sm">
                            <li>
                                <span
                                    class="inline-flex items-center justify-center w-6 h-6 bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400 rounded"
                                    >H</span
                                > View audit history.
                            </li>
                            <li>
                                <span
                                    class="inline-flex items-center justify-center w-6 h-6 bg-blue-50 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 rounded"
                                    >E</span
                                > Modify record details (Manager only).
                            </li>
                            <li>
                                <span
                                    class="inline-flex items-center justify-center w-6 h-6 bg-red-50 dark:bg-red-900/30 text-red-600 dark:text-red-400 rounded"
                                    >D</span
                                > Delete record (Manager only).
                            </li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>

        <!-- Data Tools Section -->
        <div
            class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-gray-100 dark:border-slate-700 p-6 transition-colors"
        >
            <h2
                class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2 mb-4 pb-2 border-b border-gray-100 dark:border-slate-700"
            >
                <Database class="text-emerald-500" size={24} />
                3. Import & Export
            </h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="space-y-3">
                    <h3
                        class="font-semibold text-gray-900 dark:text-white flex items-center gap-2"
                    >
                        <Download size={18} class="text-blue-500" /> Export Data
                    </h3>
                    <p class="text-sm text-gray-600 dark:text-slate-400">
                        Export your current view or selected records to:
                    </p>
                    <ul
                        class="text-sm space-y-1 list-disc pl-5 text-gray-600 dark:text-slate-400"
                    >
                        <li>
                            <strong>Excel (.xlsx):</strong> Preserves formatting
                            and styles.
                        </li>
                        <li>
                            <strong>PDF (.pdf):</strong> High-quality document for
                            sharing.
                        </li>
                    </ul>
                </div>
                <div class="space-y-3">
                    <h3
                        class="font-semibold text-gray-900 dark:text-white flex items-center gap-2"
                    >
                        <Upload size={18} class="text-emerald-500" /> Import Data
                    </h3>
                    <p class="text-sm text-gray-600 dark:text-slate-400">
                        Bulk create or update records using Excel templates.
                        Ensure column headers match the system requirements.
                    </p>
                </div>
            </div>
        </div>

        <!-- Settings Section -->
        <div
            class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-gray-100 dark:border-slate-700 p-6 transition-colors"
        >
            <h2
                class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2 mb-4 pb-2 border-b border-gray-100 dark:border-slate-700"
            >
                <Settings class="text-gray-500" size={24} />
                4. Personalization
            </h2>
            <div class="space-y-4 text-gray-600 dark:text-slate-300">
                <p>
                    Customize your experience via the Settings menu in the top
                    bar:
                </p>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div
                        class="p-3 bg-gray-50 dark:bg-slate-900/50 rounded-lg border border-gray-100 dark:border-slate-700"
                    >
                        <span
                            class="font-bold text-gray-900 dark:text-white block"
                            >Appearance</span
                        >
                        Toggle between Light and Dark mode for optimal viewing.
                    </div>
                    <div
                        class="p-3 bg-gray-50 dark:bg-slate-900/50 rounded-lg border border-gray-100 dark:border-slate-700"
                    >
                        <span
                            class="font-bold text-gray-900 dark:text-white block"
                            >Typography</span
                        >
                        Choose Small, Medium, or Large font sizes to improve readability.
                    </div>
                </div>
            </div>
        </div>

        <!-- Performance Info -->
        <div
            class="bg-blue-50 dark:bg-blue-900/20 rounded-xl p-6 border border-blue-100 dark:border-blue-800/50 flex items-start gap-4"
        >
            <Zap class="text-blue-600 dark:text-blue-400 mt-1" size={24} />
            <div>
                <h3 class="font-bold text-blue-900 dark:text-blue-200">
                    System Performance
                </h3>
                <p class="text-blue-800 dark:text-blue-300 text-sm mt-1">
                    The frontend is powered by the <strong>Bun</strong> runtime,
                    delivering lightning-fast interactions and optimized data processing.
                </p>
            </div>
        </div>

        <!-- Support Info -->
        <div
            class="bg-emerald-50 dark:bg-emerald-900/20 rounded-xl p-6 border border-emerald-100 dark:border-emerald-800/50 flex items-start gap-4"
        >
            <FileText
                class="text-emerald-600 dark:text-emerald-400 mt-1"
                size={24}
            />
            <div>
                <h3 class="font-bold text-emerald-900 dark:text-emerald-200">
                    Need more help?
                </h3>
                <p class="text-emerald-800 dark:text-emerald-300 text-sm mt-1">
                    If you encounter technical issues or persistent errors,
                    please contact the IT support team or refer to the internal
                    documentation.
                </p>
            </div>
        </div>
    </div>
</div>
