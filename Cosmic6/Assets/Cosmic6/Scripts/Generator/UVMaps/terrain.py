import numpy as np
import matplotlib.pyplot as plt
from noise import pnoise2
import struct


def generate_perlin_noise(width, height, scale, octaves, persistence, lacunarity, seed):
    noise = np.zeros((height, width))
    for y in range(height):
        for x in range(width):
            noise[y][x] = pnoise2(
                x / scale, y / scale,
                octaves=octaves,
                persistence=persistence,
                lacunarity=lacunarity,
                repeatx=width, repeaty=height,
                base=seed
            )
    return noise

def normalize(data):
    min_val = np.min(data)
    max_val = np.max(data)
    return (data - min_val) / (max_val - min_val)


def generate_fractal_terrain(width, height, scale, octaves, persistence, lacunarity, seed, weights):
    noise = np.zeros((height, width))
    amplitude = 1.0
    frequency = 1.0
    max_amplitude = 0.0

    for _ in range(octaves):
        normal_noise = generate_perlin_noise(width, height, scale * frequency, 1, 0.5, 2.0, seed)
        ridge_noise = abs(generate_perlin_noise(width, height, scale * frequency, 1, 0.5, 2.0, seed))
        valley_noise = 1 - abs(generate_perlin_noise(width, height, scale * frequency, 1, 0.5, 2.0, seed))


        noise += (normal_noise * weights[0] + ridge_noise * weights[1]  + valley_noise * weights[2]) * amplitude

        frequency *= lacunarity
        max_amplitude += amplitude
        amplitude *= persistence

    return normalize(noise)


def save_raw(data, filename):
    with open(filename, 'wb') as f:
        for value in data.flatten():
            f.write(struct.pack('<H', int(value * 25)))

def main():
    width, height = 513, 513
    scale = 100.0
    octaves = 3
    persistence = 0.5
    lacunarity = 2.0
    seed = 42

    fractal_noise = generate_fractal_terrain(
        # weights: smooth noise / noise for sharp ridge / noise for sharp crater
        width, height, scale, octaves, persistence, lacunarity, seed, [0.1, 0.4, 0.5]
    )

    heightmap = normalize(fractal_noise)

    plt.imshow(heightmap, cmap='gray')
    plt.colorbar()
    plt.title("Generated Heightmap")
    plt.show()

    save_raw(heightmap, 'alien_planet_heightmap2.raw')
    print("Heightmap saved as 'alien_planet_heightmap.raw'.")

if __name__ == "__main__":
    main()
