const path = require('path');

const webpack = require('webpack');
const HtmlPlugin = require('html-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const ReactRefreshWebpackPlugin = require('@pmmmwh/react-refresh-webpack-plugin');

module.exports = (_env, args) => {
  const prod = args.mode === "production";

  return {
    context: __dirname,
    devServer: {
      hot: true,
      host: '0.0.0.0',
      port: 3000,
      open: true,
      useLocalIp: true
    },
    devtool: prod ? 'source-map' : "inline-source-map",
    entry: [
      "./src/index"
    ],
    mode: prod ? "production" : "development",
    module: {
      rules: [
        {
          test: /\.js$/,
          exclude: /node_modules/,
          use: {
            loader: "babel-loader",
            options: {
              plugins:  prod ? [] : ["react-refresh/babel"],
            }
          }
        },
        {
          test: /\.css$/,
          use: ['style-loader', 'css-loader']
        },
        {
          test: /\.(png|gif|jpg|jpeg|svg|xml|json)$/,
          use: ['url-loader']
        },
        ...[
          prod
            ? {}
            : {}
        ]
      ]
    },
    output: {
      publicPath: '/',
      path: path.join(__dirname, "build"),
    },
    optimization: {
      splitChunks: {
        cacheGroups: {
          react:{
            test: /[\\/]node_modules[\\/](react|react-dom|react-router-dom|react-scripts|react-bootstrap)[\\/]/,
            name: 'react',
            chunks: 'all',
          },
          reactSupporting: {
            test: /[\\/]node_modules[\\/](react-toastify|reactjs-popup)[\\/]/,
            name: 'reactSupporting',
            chunks: 'all',
          },
          other:{
            test: /[\\/]node_modules[\\/](axios|moment|uuid)[\\/]/,
            name: 'other',
            chunks: 'all',
          },
          default: {
            minChunks: 2,
            priority: -20,
            reuseExistingChunk: true
          }
        }
      },
    },
    plugins: [
      new HtmlPlugin({
        template: "index.html",
      }),
      ...(prod ? [] : [new webpack.HotModuleReplacementPlugin(), new ReactRefreshWebpackPlugin()]),
    ]
  };
}