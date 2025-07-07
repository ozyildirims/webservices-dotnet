/** @type {import('next').NextConfig} */
const nextConfig = {
  experimental: {
    esmExternals: false,
  },
  images: {
    unoptimized: true,
  },
}

module.exports = nextConfig 