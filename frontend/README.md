# Port Model Manager Frontend

A modern, user-friendly frontend application for managing Port Model Mappings. Built with SvelteKit, TailwindCSS, and Lucide Icons.

## Features

-   **Dashboard View**: View all port model mappings in a responsive table.
-   **Search & Filter**: Real-time filtering by Account Sleeve or Model Name.
-   **Create Mapping**: Form to create new portfolio assignments with validation.
-   **Edit Mapping**: Update existing mappings (primary keys are immutable).
-   **Audit History**: View a history of changes for each record.
-   **Authentication**: Secure login system.
-   **Data Validation**: Form validation and error handling with user-friendly alerts.

## User Guide

### 1. Authentication
-   **Login Page**: Enter your credentials to access the system.
-   **Session**: You will remain logged in until you close the session.

### 2. Dashboard
The main dashboard gives you an overview of all your current mappings.
-   **Search Bar**: Type in the search box to filter the list instantly. It searches against both 'Account Sleeve' and 'Model Name'.
-   **Create New**: Click the **"+ Create New"** button to add a new record.

### 3. Managing Records
Each row in the table represents a mapping.
-   **Audit Log**: Click the **History (Clock)** icon to view the timeline of changes for that record.
-   **Edit**: Click the **Pencil** icon to modify the record. Note that 'Account Sleeve' and 'Effective Date' cannot be changed once created.
-   **Delete**: Click the **Trash** icon to permanently remove a record. You will be asked to confirm this action.

### 4. Forms & Input Fields

#### Create / Edit Form
-   **Account Sleeve**: The portfolio identifier.
    -   *Input*: Dropdown selection (if portfolios are available) or manual text entry.
    -   *Validation*: Required, max 20 characters.
-   **Effective Date**: The date when this mapping becomes active.
    -   *Input*: Date picker.
    -   *Validation*: Required.
-   **Model Name**: The name of the target model.
    -   *Input*: Text field.
    -   *Validation*: Required, max 50 characters.
-   **Currency Model**: Defines the currency behavior.
    -   *Input*: Dropdown selection.
    -   *Options*:
        -   `A` - Asset Model
        -   `M` - Security Model
-   **Hedge Model**: (Optional) The identifier for an associated hedge model.
    -   *Input*: Text field.
    -   *Validation*: Max 50 characters.

## Development

### Prerequisites
-   Node.js (v20 or higher recommended)
-   npm

### Installation
Install the project dependencies:
```bash
npm install
```

### Running Locally
Start the development server with hot-reload:
```bash
npm run dev
```
The app will be available at `http://localhost:5173`.

### Type Checking
Run a check for TypeScript errors:
```bash
npm run check
```

## Production Build

### Building the App
Create a production build:
```bash
npm run build
```
The output will be in the `build/` directory.

### Preview Build
Preview the production build locally:
```bash
npm run preview
```

## Docker

### Build Container
Build the Docker image:
```bash
docker build -t frontend .
```

### Run Container
Run the container, mapping port 3000:
```bash
docker run -p 3000:3000 -e ORIGIN=http://localhost:3000 frontend
```
The application will be accessible at `http://localhost:3000`.

### Environment Variables
-   `PORT`: The port the server listens on (default: 3000).
-   `ORIGIN`: The origin URL of the app (required for CORS in production).
