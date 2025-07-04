# Use official Node.js 20 image
FROM node:20-alpine AS base
WORKDIR /app

# Allow self-signed certs for npm only during build
ENV NODE_TLS_REJECT_UNAUTHORIZED=0

# Install dependencies and build
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# Production image, copy only necessary files (no insecure env)
FROM node:20-alpine AS prod
WORKDIR /app
COPY --from=base /app .
EXPOSE 3000
CMD ["npm", "start"] 