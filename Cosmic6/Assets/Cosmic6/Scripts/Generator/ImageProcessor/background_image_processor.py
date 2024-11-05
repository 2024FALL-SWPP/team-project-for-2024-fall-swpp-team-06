import cv2
import numpy as np
import time
import math
from PIL import Image
from sympy.combinatorics.graycode import graycode_subsets

source = cv2.imread('planet2.jpg')


(h, w) = source.shape[:2]
center = (w // 2, h // 2)

angle = -35
scale = 1.0
rotation_matrix = cv2.getRotationMatrix2D(center, angle, scale)

rotation_matrix2 = cv2.getRotationMatrix2D(center, 10, scale)
rotated_source = cv2.warpAffine(source, rotation_matrix, (w, h))

rotated_source2 = cv2.warpAffine(source, rotation_matrix2, (w, h))

source2 = cv2.imread('planet.jpg')

gray_source2 = cv2.cvtColor(rotated_source2, cv2.COLOR_BGR2GRAY)

scale_factor = 2/3

new_width = int(scale_factor * w)
new_height = new_width

print(new_width, new_height)

gray_source2 = cv2.resize(gray_source2, (new_width, new_height), interpolation=cv2.INTER_LANCZOS4)

gray_source = cv2.cvtColor(rotated_source, cv2.COLOR_BGR2GRAY)

alpha = 0.7

gray_background = np.full_like(gray_source, 128)
gray_source = cv2.addWeighted(gray_source, alpha, gray_background, 1 - alpha, 0)

gray_background2 = np.full_like(gray_source2, 128)
gray_source2 = cv2.addWeighted(gray_source2, alpha, gray_background2, 1 - alpha, 0)

# blurred_source = cv2.GaussianBlur(gray_source, (0, 0), 0)


blurred_source = cv2.GaussianBlur(gray_source, (0, 0), math.sqrt(1.75))
blurred_source2 = cv2.GaussianBlur(gray_source2, (0, 0), math.sqrt(33.75))

size_factor = 2/3

background = np.zeros((int(2048*size_factor), 6*int(2048*size_factor), 3), np.uint8)

x, y = int(1024*size_factor), int(500*size_factor)

mask = (gray_source > 56).astype(np.float32)
mask2 = (gray_source2 > 52).astype(np.float32)


blurred_mask = cv2.GaussianBlur(mask, (0, 0), 6) * mask
blurred_mask2 = cv2.GaussianBlur(mask2, (0, 0), 4) * mask2



x2, y2 = int(900*size_factor), int(300*size_factor)


background[y2:y2+gray_source2.shape[0], x2:x2+gray_source2.shape[1]] =\
    cv2.cvtColor((blurred_mask2 * blurred_source2).astype(np.uint8), cv2.COLOR_GRAY2RGB)

background[y:y+source.shape[0], x:x+source.shape[1]] = \
    (blurred_mask[..., np.newaxis] * cv2.cvtColor(blurred_source, cv2.COLOR_GRAY2RGB)
     + (1 - blurred_mask)[..., np.newaxis] * background[y:y+source.shape[0], x:x+source.shape[1]]).astype(np.uint8)

gaussian_mask = (background > 0)[..., 0]
background = cv2.GaussianBlur(background, (0, 0), 1.5)



'''
background[y2:y2+source2.shape[0], x2:x2+source2.shape[1]] = np.where(
    mask2[..., np.newaxis],
    cv2.cvtColor(blurred_source2, cv2.COLOR_GRAY2RGB),
    background[y2:y2+source2.shape[0], x2:x2+source2.shape[1]]
)


background[y:y+source.shape[0], x:x+source.shape[1]] = np.where(
    mask[..., np.newaxis],
    cv2.cvtColor(blurred_source, cv2.COLOR_GRAY2RGB),
    background[y:y+source.shape[0], x:x+source.shape[1]]
)
'''

planet_mask = cv2.GaussianBlur((gaussian_mask).astype(np.uint8), (0, 0), 25) * cv2.GaussianBlur((gaussian_mask).astype(np.uint8), (0, 0), 2)
planet_mask = cv2.cvtColor(planet_mask * 255, cv2.COLOR_GRAY2BGR)

# planet_mask = cv2.cvtColor(gaussian_mask.astype(np.uint8) * 255, cv2.COLOR_GRAY2BGR)
cv2.imwrite("./blended_HandDoneJPEG2.jpg", background)
cv2.imwrite("./planet_mask2.jpg", planet_mask)



'''
star = cv2.imread('./Stars Small_1.png', cv2.IMREAD_UNCHANGED)
background = cv2.imread('./Nebula Aqua-Pink.png', cv2.IMREAD_UNCHANGED)


dup_star = np.zeros_like(star)
dup_background = np.zeros_like(background)

down_star = cv2.pyrDown(star)
down_background = cv2.pyrDown(background)

img_size = 2048

dup_star[:img_size, :img_size] = down_star
dup_star[img_size:, :img_size] = down_star
dup_star[img_size:, img_size:] = down_star
dup_star[:img_size, img_size:] = down_star

dup_background[:img_size, :img_size] = down_background
dup_background[img_size:, :img_size] = down_background
dup_background[img_size:, img_size:] = down_background
dup_background[:img_size, img_size:] = down_background

alpha_mask = dup_star[..., 3:] / 255.0

blended = np.clip(((1 - alpha_mask) + dup_background + alpha_mask * dup_star), 0, 255).astype(np.uint8)

cv2.imwrite("blended.png", blended)
'''
