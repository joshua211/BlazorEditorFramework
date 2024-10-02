module.exports = {
    darkMode: 'media', // or 'media' or false
    content: [
        '../**/*.razor',
        '../**/*.html',
    ],
    theme: {
        extend: {
            gridTemplateColumns: {
                // Simple 16 column grid
                '16': 'repeat(16, minmax(0, 1fr))'
            }
        },
    },
    plugins: [],
}