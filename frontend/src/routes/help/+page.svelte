<script>
    import { ArrowLeft, BookOpen, Shield, LayoutDashboard, Database, FileText } from "lucide-svelte";
</script>

<div class="container mx-auto px-4 py-8 max-w-4xl">
    <div class="mb-8 flex items-center justify-between">
        <div>
            <a
                href="/"
                class="text-gray-500 hover:text-gray-800 flex items-center gap-2 mb-2 transition-colors"
            >
                <ArrowLeft size={20} />
                Back to Dashboard
            </a>
            <h1 class="text-3xl font-bold text-gray-900 flex items-center gap-3">
                <BookOpen class="text-blue-600" />
                User Manual
            </h1>
            <p class="text-gray-500 mt-2">Complete guide to using the Port Model Manager.</p>
        </div>
    </div>

    <div class="space-y-8">
        <!-- Authentication Section -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-xl font-bold text-gray-900 flex items-center gap-2 mb-4 pb-2 border-b border-gray-100">
                <Shield class="text-indigo-500" size={24} />
                1. Authentication
            </h2>
            <div class="space-y-4 text-gray-600">
                <p>
                    <strong class="text-gray-900">Login:</strong> Access the system by entering your username and password on the login page.
                </p>
                <p>
                    <strong class="text-gray-900">Session:</strong> Your session remains active until you click the Logout button in the top right corner of the application.
                </p>
            </div>
        </div>

        <!-- Dashboard Section -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-xl font-bold text-gray-900 flex items-center gap-2 mb-4 pb-2 border-b border-gray-100">
                <LayoutDashboard class="text-blue-500" size={24} />
                2. Dashboard Usage
            </h2>
            <div class="space-y-4 text-gray-600">
                <ul class="list-disc pl-5 space-y-2">
                    <li>
                        <strong class="text-gray-900">View Mappings:</strong> The main dashboard displays a comprehensive table of all Port Model Mappings.
                    </li>
                    <li>
                        <strong class="text-gray-900">Search:</strong> Use the search bar at the top to filter records instantly. You can search by <em>Account Sleeve</em> or <em>Model Name</em>.
                    </li>
                    <li>
                        <strong class="text-gray-900">Create New:</strong> Click the <span class="px-2 py-0.5 bg-blue-600 text-white rounded text-xs font-medium">+ Create New</span> button to navigate to the creation form.
                    </li>
                    <li>
                        <strong class="text-gray-900">Actions:</strong>
                        <ul class="list-circle pl-5 mt-1 space-y-1 text-sm">
                            <li><span class="inline-flex items-center justify-center w-6 h-6 bg-indigo-50 text-indigo-600 rounded">H</span> <strong>History:</strong> View the audit log for changes.</li>
                            <li><span class="inline-flex items-center justify-center w-6 h-6 bg-blue-50 text-blue-600 rounded">E</span> <strong>Edit:</strong> Modify the record details.</li>
                            <li><span class="inline-flex items-center justify-center w-6 h-6 bg-red-50 text-red-600 rounded">D</span> <strong>Delete:</strong> Permanently remove the record.</li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>

        <!-- Managing Records Section -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-xl font-bold text-gray-900 flex items-center gap-2 mb-4 pb-2 border-b border-gray-100">
                <Database class="text-emerald-500" size={24} />
                3. Managing Records
            </h2>
            <div class="space-y-6">
                <div>
                    <h3 class="font-semibold text-gray-900 mb-2">Creating a Record</h3>
                    <p class="text-gray-600 mb-2">Fill out the form with the following details:</p>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div class="bg-gray-50 p-3 rounded-lg border border-gray-200">
                            <span class="block text-xs font-bold text-gray-500 uppercase">Account Sleeve</span>
                            <span class="text-sm text-gray-800">Portfolio identifier. Select from dropdown or type manually. (Max 20 chars)</span>
                        </div>
                        <div class="bg-gray-50 p-3 rounded-lg border border-gray-200">
                            <span class="block text-xs font-bold text-gray-500 uppercase">Effective Date</span>
                            <span class="text-sm text-gray-800">The date when this mapping becomes active.</span>
                        </div>
                        <div class="bg-gray-50 p-3 rounded-lg border border-gray-200">
                            <span class="block text-xs font-bold text-gray-500 uppercase">Model Name</span>
                            <span class="text-sm text-gray-800">Target model identifier. (Max 50 chars)</span>
                        </div>
                        <div class="bg-gray-50 p-3 rounded-lg border border-gray-200">
                            <span class="block text-xs font-bold text-gray-500 uppercase">Currency Model</span>
                            <span class="text-sm text-gray-800">Choose 'A' for Asset Model or 'M' for Security Model.</span>
                        </div>
                    </div>
                </div>

                <div>
                    <h3 class="font-semibold text-gray-900 mb-2">Editing a Record</h3>
                    <p class="text-gray-600 border-l-4 border-yellow-400 pl-4 py-1 bg-yellow-50">
                        <span class="font-bold">Note:</span> The <em>Account Sleeve</em> and <em>Effective Date</em> are primary keys and cannot be changed once created. To "move" a mapping, you must delete the old one and create a new one.
                    </p>
                </div>
            </div>
        </div>
        
        <!-- Support Info -->
         <div class="bg-blue-50 rounded-xl p-6 border border-blue-100 flex items-start gap-4">
            <FileText class="text-blue-600 mt-1" size={24} />
            <div>
                <h3 class="font-bold text-blue-900">Need more help?</h3>
                <p class="text-blue-800 text-sm mt-1">
                    This detailed manual provides an overview of all system features. If you encounter technical issues or persistent errors, please contact the IT support team.
                </p>
            </div>
         </div>
    </div>
</div>
