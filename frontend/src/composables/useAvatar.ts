import { ref, computed } from 'vue'
import type { Profile } from '@/types'

export const useAvatar = () => {
    const getInitials = (username?: string): string => {
        if (!username) return 'U';
        return username.charAt(0).toUpperCase();
    }

    const getAvatarUrl = (profile?: Profile | null): string | null => {
        return profile?.avatar || null;
    }

    const getAvatarDisplay = (profile?: Profile | null) => {
        return {
            hasAvatar: !!profile?.avatar,
            avatarUrl: profile?.avatar || null,
            initials: getInitials(profile?.username),
            username: profile?.username || 'Unknown'
        }
    }

    return {
        getInitials,
        getAvatarUrl,
        getAvatarDisplay
    }
}
