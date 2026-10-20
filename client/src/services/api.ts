import axios from 'axios'

// Defaults to the API's `http` launch profile (api/Properties/launchSettings.json)
export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5129'

export const api = axios.create({ baseURL: API_URL })
