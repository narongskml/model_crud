import { type Snippet } from 'svelte';

class HeaderState {
    actions = $state<Snippet | undefined>(undefined);
}

export const headerState = new HeaderState();
