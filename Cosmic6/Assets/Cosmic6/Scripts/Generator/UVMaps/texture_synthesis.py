import cv2
import numpy as np
import random
import os
from tqdm import tqdm

os.environ["OPENCV_IO_ENABLE_OPENEXR"] = "1"

def load_texture(file_path):
    img = cv2.imread(file_path, cv2.IMREAD_UNCHANGED)

    if img is None:
        raise ValueError(f"Failed to load {file_path}")

    if img.dtype == np.float32:
        return img
    else:
        return img.astype(np.float32) / 255.0


def min_cut_path(overlap1, overlap2):
    h = min(overlap1.shape[0], overlap2.shape[0])
    w = min(overlap1.shape[1], overlap2.shape[1])


    diff = np.sum((overlap1[:h,:w] - overlap2[:h, :w]) ** 2, axis=2)
    cost = np.zeros_like(diff)
    cost[0, :] = diff[0, :]

    for i in range(1, diff.shape[0]):
        for j in range(diff.shape[1]):
            min_cost = cost[i - 1, max(0, j - 1):min(j + 2, diff.shape[1])].min()
            cost[i, j] = diff[i, j] + min_cost

    path = np.zeros(diff.shape[0], dtype=np.int32)
    path[-1] = np.argmin(cost[-1])
    for i in range(diff.shape[0] - 2, -1, -1):
        previous = path[i + 1]
        path[i] = np.argmin(cost[i, max(0, previous - 1):min(previous + 2, diff.shape[1])]) + max(0, previous - 1)

    return path


def texture_quilt(input_files, in_patch_size=512, out_patch_size=128, output_size=4096, overlap=64):
    textures = [load_texture(f) for f in input_files]
    outputs = [np.zeros((output_size, output_size, 3), dtype=dtype)
                    for dtype in map(lambda x: x.dtype,textures)]

    step = out_patch_size - overlap

    for i in tqdm(range(0, output_size, step)):
        for j in range(0, output_size, step):
            tx = random.randint(0, textures[0].shape[1] - in_patch_size)
            ty = random.randint(0, textures[0].shape[0] - in_patch_size)

            patch_diff = textures[0][tx: tx + in_patch_size, ty: ty + in_patch_size]
            downsampled_patch_diff = downsample_patch(patch_diff, out_patch_size)

            over_y = max(0, i + out_patch_size - output_size)
            over_x = max(0, j + out_patch_size - output_size)

            if i > 0:
                vertical_overlap_diff = outputs[0][i:i + overlap, j:j + out_patch_size]
                cut = min_cut_path(vertical_overlap_diff, downsampled_patch_diff[:overlap, :])

                if j == 0:
                    outputs[0][i:i + out_patch_size - over_y, j:j + out_patch_size - over_x] =\
                        downsampled_patch_diff[:out_patch_size - over_y, :out_patch_size - over_x]

                for k in range(overlap):
                    if i + k < output_size and j + out_patch_size < output_size:
                        outputs[0][i + k, j:j + out_patch_size] = np.where(
                            (np.arange(out_patch_size) < cut[k])[..., np.newaxis],
                            vertical_overlap_diff[k, :],
                            downsampled_patch_diff[k, :]
                        )

                        for l, texture in enumerate(textures[1:]):

                            patch = textures[l][tx: tx + in_patch_size, ty: ty + in_patch_size]
                            downsampled_patch = downsample_patch(patch, out_patch_size)
                            vertical_overlap = outputs[l][i:i + overlap, j:j + out_patch_size]

                            if j == 0:
                                outputs[l][i:i + out_patch_size - over_y, j:j + out_patch_size - over_x] =\
                                    downsampled_patch[:out_patch_size - over_y, :out_patch_size - over_x]

                            outputs[l][i + k, j:j + out_patch_size] = np.where(
                                (np.arange(out_patch_size) < cut[k])[..., np.newaxis],
                                vertical_overlap[k, :],
                                downsampled_patch[k, :]
                            )
            if j > 0:
                horizontal_overlap = outputs[0][i:i + out_patch_size, j:j + overlap]

                cut = min_cut_path(horizontal_overlap.transpose(1, 0, 2),
                                   downsampled_patch_diff[:, :overlap].transpose(1, 0, 2))

                outputs[0][i:i + out_patch_size - over_y, j:j + out_patch_size - over_x] = \
                    downsampled_patch_diff[:out_patch_size - over_y, :out_patch_size - over_x]

                for k in range(overlap):
                    if i + out_patch_size < output_size and j + out_patch_size < output_size:
                        outputs[0][i:i + out_patch_size, j + k] = np.where(
                            (np.arange(out_patch_size) < cut[k])[..., np.newaxis],
                            horizontal_overlap[:, k],
                            downsampled_patch_diff[:, k]
                        )

                        for l, texture in enumerate(textures[1:]):
                            patch = textures[l][tx: tx + in_patch_size, ty: ty + in_patch_size]
                            downsampled_patch = downsample_patch(patch, out_patch_size)
                            horizontal_overlap = outputs[l][i:i + out_patch_size, j:j + overlap]

                            outputs[l][i:i + out_patch_size - over_y, j:j + out_patch_size - over_x] = \
                                downsampled_patch[:out_patch_size - over_y, :out_patch_size - over_x]

                            outputs[l][i:i + out_patch_size, j + k] = np.where(
                                (np.arange(out_patch_size) < cut[k])[..., np.newaxis],
                                horizontal_overlap[:, k],
                                downsampled_patch[:, k]
                            )

            if i == 0 and j == 0:
                outputs[0][i:i + out_patch_size, j:j + out_patch_size] = downsampled_patch_diff
                for l, texture in enumerate(textures[1:]):
                    patch = textures[l][tx: tx + in_patch_size, ty: ty + in_patch_size]
                    downsampled_patch = downsample_patch(patch, out_patch_size)
                    outputs[l][i:i + out_patch_size, j:j + out_patch_size] = downsampled_patch

    return outputs


def texture_quilt2(input_files, in_patch_size=512, out_patch_size=128, output_size=4096, overlap_factor=2, laplacian_level=5):
    overlap = out_patch_size // overlap_factor

    dtypes = [np.uint8, np.float32, np.float32]
    textures = [load_texture(f) for f in input_files]
    outputs = [np.zeros((output_size, output_size, 3), dtype=np.float32) for _ in textures]

    x, y = np.meshgrid(np.arange(out_patch_size), np.arange(out_patch_size))

    mask_2d = np.where(x > y, np.clip(1 - y / (overlap - 1), 0, 1), np.clip(1 - x / (overlap - 1), 0, 1))

    mask_1d_x = np.clip(1 - x / (overlap - 1), 0, 1)

    step = out_patch_size - overlap

    for i in tqdm(range(0, output_size, step)):
        for j in range(0, output_size, step):
            tx = random.randint(0, textures[0].shape[1] - in_patch_size)
            ty = random.randint(0, textures[0].shape[0] - in_patch_size)

            over_y = max(0, i + out_patch_size - output_size)
            over_x = max(0, j + out_patch_size - output_size)


            for k, texture in enumerate(textures):
                patch = texture[tx: tx + in_patch_size, ty: ty + in_patch_size]
                downsampled_patch = downsample_patch(patch, out_patch_size)

                if i == 0 and j == 0:
                    outputs[k][i:i + out_patch_size, j:j + out_patch_size] = downsampled_patch

                else:
                    base_patch = outputs[k][i:i + out_patch_size - over_y, j:j + out_patch_size - over_x]
                    downsampled_patch = downsampled_patch[:out_patch_size - over_y, :out_patch_size - over_x]

                    if i > 0 and j > 0:
                        mask = mask_2d > 0

                        blended = laplacian_pyramid_blending(
                            base_patch, downsampled_patch,
                            mask_2d[:out_patch_size - over_y, :out_patch_size - over_x],
                            levels=laplacian_level
                        )
                    elif i == 0:
                        mask = mask_1d_x > 0
                        blended = laplacian_pyramid_blending(
                            base_patch, downsampled_patch,
                            mask_1d_x[:out_patch_size - over_y, :out_patch_size - over_x],
                            levels=laplacian_level
                        )
                    else:
                        mask = mask_1d_x.T > 0
                        blended = laplacian_pyramid_blending(
                            base_patch, downsampled_patch,
                            mask_1d_x.T[:out_patch_size - over_y, :out_patch_size - over_x],
                            levels=laplacian_level
                        )

                    outputs[k][i:i + out_patch_size - over_y, j:j + out_patch_size - over_x] = downsampled_patch
                    outputs[k][i:i + out_patch_size - over_y, j:j + out_patch_size - over_x][mask[:out_patch_size - over_y,:out_patch_size - over_x]]\
                        = blended[mask[:out_patch_size - over_y,:out_patch_size - over_x]]

    return [((255 if dtype == np.uint8 else 1) * output).astype(dtype) for dtype, output in zip(dtypes, outputs)]


def build_gaussian_pyramid(img, levels=6):
    pyramid = [img]
    for _ in range(levels):
        img = cv2.pyrDown(img)
        pyramid.append(img)
    return pyramid


def build_laplacian_pyramid(img, levels=6):
    gaussian_pyramid = build_gaussian_pyramid(img, levels)
    laplacian_pyramid = []

    for i in range(levels):
        expanded = cv2.pyrUp(gaussian_pyramid[i + 1])
        expanded = cv2.resize(expanded, (gaussian_pyramid[i].shape[1], gaussian_pyramid[i].shape[0]))
        laplacian = cv2.subtract(gaussian_pyramid[i], expanded)
        laplacian_pyramid.append(laplacian)

    laplacian_pyramid.append(gaussian_pyramid[-1])
    return laplacian_pyramid

def laplacian_pyramid_blending(img1, img2, mask, levels=6):
    lp_img1 = build_laplacian_pyramid(img1, levels)
    lp_img2 = build_laplacian_pyramid(img2, levels)

    gp_mask = build_gaussian_pyramid(mask, levels)

    blended_pyramid = []
    for la, lb, gm in zip(lp_img1, lp_img2, gp_mask):
        gm = gm[..., np.newaxis]
        blended = la * gm + lb * (1 - gm)
        blended_pyramid.append(blended)

    blended_img = blended_pyramid[-1]
    for i in range(levels - 2, -1, -1):
        blended_img = cv2.pyrUp(blended_img)
        blended_img = cv2.resize(blended_img, (blended_pyramid[i].shape[1], blended_pyramid[i].shape[0]))
        blended_img += blended_pyramid[i]

    return blended_img


def downsample_patch(patch, target_size):
    return cv2.resize(patch, (target_size, target_size), interpolation=cv2.INTER_LANCZOS4)


def blend_patch(base, patch, x, y, boundary):
    h, w, _ = patch.shape
    mask = np.tile(boundary[:, :, None], [1, 1, 3])
    base[y:y + h, x:x + w] = mask * patch + (1 - mask) * base[y:y + h, x:x + w]

def sharpen_images(images, blur_sigma, sharpen_alpha):
    result = []
    for image in images:
        dtype = image.dtype
        img = image.astype(np.float32)
        blurred_image = cv2.GaussianBlur(img, (0, 0), blur_sigma)
        sharpend_image = cv2.addWeighted(img, 1 + sharpen_alpha, blurred_image, -sharpen_alpha, 0)

        if dtype == np.uint8:
            sharpend_image = np.clip(sharpend_image, 0, 255)

        result.append(sharpend_image.astype(dtype))
    return result


def save_output_images(images, output_dir):
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    names = ["diffuse", "arm", "normal"]
    for i, img in enumerate(images):
        if img.dtype == np.float32:
            save_path = os.path.join(output_dir, f"{names[i]}_map.exr")
            cv2.imwrite(save_path, img)
        else:
            save_path = os.path.join(output_dir, f"{names[i]}_map.jpg")
            cv2.imwrite(save_path, img, [cv2.IMWRITE_JPEG_QUALITY, 100])

input_files = [
    "./rock_face_03_diff_4k.jpg",  # Diffuse Map (JPG)
    "./rock_face_03_arm_4k.exr",      # ORM Map (EXR)
    "./rock_face_03_nor_gl_4k.exr"    # Normal Map (EXR)
]

output_dir = "output_textures"

output_images = texture_quilt2(input_files)

# save_output_images(sharpen_images(output_images, 4, 0.5), output_dir)
save_output_images(output_images, output_dir)