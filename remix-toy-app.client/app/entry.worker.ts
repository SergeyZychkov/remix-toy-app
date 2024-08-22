
import {
 EnhancedCache,
 isDocumentRequest,
 isLoaderRequest,
 Logger,
 type DefaultFetchHandler,
} from '@remix-pwa/sw'

import { NavigationHandler } from '@remix-pwa/sw'

// rest of the service worker


const version = 'v1'

const DOCUMENT_CACHE_NAME = `document-cache`;
const ASSET_CACHE_NAME = `asset-cache`;
const DATA_CACHE_NAME = `data-cache`;

const documentCache = new EnhancedCache(DOCUMENT_CACHE_NAME, {
  version,
  strategy: 'CacheFirst',
  strategyOptions: {
    maxEntries: 64,
  }
})

const assetCache = new EnhancedCache(ASSET_CACHE_NAME, {
  version,
  strategy: 'CacheFirst',
  strategyOptions: {
    maxAgeSeconds: 60 * 60 * 24 * 90, // 90 days
    maxEntries: 100,
  }
})

const dataCache = new EnhancedCache(DATA_CACHE_NAME, {
  version,
  strategy: 'NetworkFirst',
  strategyOptions: {
    networkTimeoutInSeconds: 10,
    maxEntries: 72,
  }
})


// rest of our service worker

export const defaultFetchHandler: DefaultFetchHandler = async ({ context }) => {
 const request = context.event.request
 const url = new URL(request.url)

 if (isDocumentRequest(request)) {
  return documentCache.handleRequest(request)
 }

 if (isLoaderRequest(request)) {
  return dataCache.handleRequest(request)
 }

 return fetch(request)
}


const messageHandler = new NavigationHandler({
  cache: documentCache
})

self.addEventListener('message', (event: any) => {
  event.waitUntil(messageHandler.handleMessage(event))
})