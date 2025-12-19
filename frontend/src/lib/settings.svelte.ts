import { browser } from '$app/environment';

type Theme = 'light' | 'dark';
type FontSize = 'small' | 'medium' | 'large';

class SettingsState {
    theme = $state<Theme>('light');
    fontSize = $state<FontSize>('medium');

    constructor() {
        if (browser) {
            // Load from local storage
            const savedTheme = localStorage.getItem('theme') as Theme;
            const savedFontSize = localStorage.getItem('fontSize') as FontSize;

            if (savedTheme) this.theme = savedTheme;
            else if (window.matchMedia('(prefers-color-scheme: dark)').matches) this.theme = 'dark';

            if (savedFontSize) this.fontSize = savedFontSize;
        }
    }

    setTheme(newTheme: Theme) {
        this.theme = newTheme;
        if (browser) {
            localStorage.setItem('theme', newTheme);
            if (newTheme === 'dark') {
                document.documentElement.classList.add('dark');
            } else {
                document.documentElement.classList.remove('dark');
            }
        }
    }

    setFontSize(newSize: FontSize) {
        this.fontSize = newSize;
        if (browser) {
            localStorage.setItem('fontSize', newSize);

            // Remove all font size classes
            document.documentElement.classList.remove('text-sm', 'text-base', 'text-lg');

            // Add new class
            switch (newSize) {
                case 'small':
                    document.documentElement.classList.add('text-sm');
                    break;
                case 'medium':
                    document.documentElement.classList.add('text-base');
                    break;
                case 'large':
                    document.documentElement.classList.add('text-lg');
                    break;
            }
        }
    }

    // Initialize Global Styles on mount/load
    init() {
        if (browser) {
            this.setTheme(this.theme);
            this.setFontSize(this.fontSize);
        }
    }
}

export const settings = new SettingsState();
