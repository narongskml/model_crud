import { writable } from 'svelte/store';
import { browser } from '$app/environment';

interface User {
    username: string;
}

const storedToken = browser ? localStorage.getItem('token') : null;
const storedUser = browser ? localStorage.getItem('user') : null;

export const token = writable<string | null>(storedToken);
export const user = writable<User | null>(storedUser ? JSON.parse(storedUser) : null);

if (browser) {
    token.subscribe(value => {
        if (value) localStorage.setItem('token', value);
        else localStorage.removeItem('token');
    });

    user.subscribe(value => {
        if (value) localStorage.setItem('user', JSON.stringify(value));
        else localStorage.removeItem('user');
    });
}

export function logout() {
    token.set(null);
    user.set(null);
    if (browser) window.location.href = '/login';
}
