// Rasterizes the master brand SVG (assets/icon.svg) into the PNGs NuGet + the docs favicon need.
// Run from anywhere: `node docs/scripts/gen-icon.mjs` (resolves sharp from docs/node_modules).
import sharp from 'sharp';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const root = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
const icon = readFileSync(join(root, 'assets', 'icon.svg'));
const og = readFileSync(join(root, 'assets', 'og.svg'));

// Square, transparent icons/favicons.
const squares = [
  ['assets/icon.png', 128],            // NuGet PackageIcon
  ['assets/icon-256.png', 256],        // higher-res spare (social, README)
  ['docs/public/favicon.png', 180],    // apple-touch + PNG fallback
  ['docs/public/favicon-32.png', 32],  // classic favicon fallback
];
for (const [rel, size] of squares) {
  await sharp(icon, { density: 512 })
    .resize(size, size, { fit: 'contain', background: { r: 0, g: 0, b: 0, alpha: 0 } })
    .png()
    .toFile(join(root, rel));
  console.log(`wrote ${rel} (${size}px)`);
}

// Open Graph / social-preview banner (1280x640), rendered 2x then downsampled for crisp text.
for (const rel of ['assets/og.png', 'docs/public/og.png']) {
  await sharp(og, { density: 192 }).resize(1280, 640).png().toFile(join(root, rel));
  console.log(`wrote ${rel} (1280x640)`);
}
