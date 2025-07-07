# Use official Node.js 20 image
FROM node:20-alpine
WORKDIR /app

# Allow self-signed certs for npm only during build
ENV NODE_TLS_REJECT_UNAUTHORIZED=0

# Set build-time environment variable
ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

# Install dependencies and build
COPY package.json ./
RUN npm install
COPY . .
RUN npm run build

EXPOSE 3000
CMD ["npm", "start"] 