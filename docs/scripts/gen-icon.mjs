// Rasterizes the master brand SVG (assets/icon.svg) into the PNGs NuGet + the docs favicon need.
// Run from anywhere: `node docs/scripts/gen-icon.mjs` (resolves sharp from docs/node_modules).
import sharp from 'sharp';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const root = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
const svg = readFileSync(join(root, 'assets', 'icon.svg'));

const jobs = [
  ['assets/icon.png', 128],            // NuGet PackageIcon
  ['assets/icon-256.png', 256],        // higher-res spare (social, README)
  ['docs/public/favicon.png', 180],    // apple-touch + PNG fallback
  ['docs/public/favicon-32.png', 32],  // classic favicon fallback
];

for (const [rel, size] of jobs) {
  await sharp(svg, { density: 512 })
    .resize(size, size, { fit: 'contain', background: { r: 0, g: 0, b: 0, alpha: 0 } })
    .png()
    .toFile(join(root, rel));
  console.log(`wrote ${rel} (${size}px)`);
}
