import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../Selu383.SP26.Api/wwwroot',
    emptyOutDir: true,
  },
})
