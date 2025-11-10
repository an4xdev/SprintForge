export const getTaskStatusColor = (statusName: string): string => {
    const colors: Record<string, string> = {
        'Created': 'blue-grey',
        'Assigned': 'blue',
        'Started': 'orange',
        'Paused': 'amber',
        'Stopped': 'red',
        'Completed': 'green',
    };
    return colors[statusName] || 'grey';
};

export const getTaskTypeColor = (typeName: string): string => {
    const colors: Record<string, string> = {
        'Feature': 'green',
        'Bug': 'red',
        'Improvement': 'blue',
        'Research': 'purple',
        'Task': 'grey',
    };
    return colors[typeName] || 'grey';
};
