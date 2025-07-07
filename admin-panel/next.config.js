/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  experimental: {
    esmExternals: false,
  },
  images: {
    unoptimized: true,
  },
}

module.exports = nextConfig 