const path = require('path');

module.exports = {
    entry: {
        main: './src/index.ts',
    },
    // 
    module: {
        rules: [
            {
                test: /\.ts?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    output: {
        filename: 'index.bundle.js',
        path: path.resolve(__dirname, '../wwwroot/js'),
    },
    optimization: {
        minimize: false
    },
};