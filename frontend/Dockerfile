# Build Stage
FROM oven/bun:alpine AS build
WORKDIR /app

COPY package.json bun.lock ./
RUN bun install --frozen-lockfile

COPY . .
RUN bun run build

# Production Dependencies Stage
FROM oven/bun:alpine AS prod-deps
WORKDIR /app
COPY package.json bun.lock ./
RUN bun install --frozen-lockfile --production

# Runtime Stage
FROM oven/bun:alpine
WORKDIR /app

# Copy built assets and only production deps
COPY --from=build /app/build ./build
COPY --from=prod-deps /app/node_modules ./node_modules
COPY --from=prod-deps /app/package.json ./package.json

EXPOSE 3000
ENV NODE_ENV=production
ENV PORT=3000
ENV ORIGIN=http://localhost:3000

# Run with Bun
CMD ["bun", "run", "build/index.js"]
