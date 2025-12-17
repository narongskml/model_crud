# Build Stage
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build
RUN npm prune --production

# Runtime Stage
FROM node:20-alpine
WORKDIR /app

# Copy built assets and production deps
COPY --from=build /app/build ./build
COPY --from=build /app/package.json ./package.json
COPY --from=build /app/node_modules ./node_modules

EXPOSE 3000
ENV NODE_ENV=production
ENV PORT=3000
# Allow connection to backend
ENV ORIGIN=http://localhost:3000

CMD ["node", "build"]
