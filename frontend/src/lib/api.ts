import type { PortModelMapping, PortModelMappingAudit, CreateResponse } from './types';
import { get } from 'svelte/store';
import { token } from './auth';

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5137/api';

function getHeaders() {
    const t = get(token);
    return {
        'Content-Type': 'application/json',
        ...(t ? { 'Authorization': `Bearer ${t}` } : {})
    };
}

export const api = {
    async login(username: string, password: string): Promise<{ token: string, username: string }> {
        const res = await fetch(`${API_BASE}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });
        if (!res.ok) throw new Error('Login failed');
        return res.json();
    },

    async getPortfolios(): Promise<{ code: string, name: string }[]> {
        const res = await fetch(`${API_BASE}/Portfolios`, { headers: getHeaders() });
        if (!res.ok) throw new Error('Failed to fetch portfolios');
        return res.json();
    },

    async getMappings(): Promise<PortModelMapping[]> {
        const res = await fetch(`${API_BASE}/PortModelMappings`, { headers: getHeaders() });
        if (!res.ok) throw new Error('Failed to fetch mappings');
        return res.json();
    },

    async getMapping(accno: string, date: string): Promise<PortModelMapping> {
        const res = await fetch(`${API_BASE}/PortModelMappings/${accno}/${date}`, { headers: getHeaders() });
        if (!res.ok) throw new Error('Failed to fetch mapping');
        return res.json();
    },

    async createMapping(data: PortModelMapping): Promise<CreateResponse> {
        const res = await fetch(`${API_BASE}/PortModelMappings`, {
            method: 'POST',
            headers: getHeaders(),
            body: JSON.stringify(data)
        });
        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.message || 'Failed to create record');
        }
        return res.json();
    },

    async updateMapping(accno: string, date: string, data: PortModelMapping): Promise<CreateResponse | void> {
        const res = await fetch(`${API_BASE}/PortModelMappings/${accno}/${date}`, {
            method: 'PUT',
            headers: getHeaders(),
            body: JSON.stringify(data)
        });
        if (!res.ok) {
            if (res.status === 404) throw new Error('Record not found');
            const err = await res.json().catch(() => ({}));
            throw new Error(err.message || 'Failed to update record');
        }
        if (res.status === 200) return res.json();
    },

    async deleteMapping(accno: string, date: string): Promise<void> {
        const res = await fetch(`${API_BASE}/PortModelMappings/${accno}/${date}`, {
            method: 'DELETE',
            headers: getHeaders()
        });
        if (!res.ok) throw new Error('Failed to delete record');
    },

    async getAuditHistory(accno: string, date: string): Promise<PortModelMappingAudit[]> {
        const res = await fetch(`${API_BASE}/PortModelMappingAudits/${accno}/${date}`, { headers: getHeaders() });
        if (!res.ok) throw new Error('Failed to fetch audit history');
        return res.json();
    }
};

