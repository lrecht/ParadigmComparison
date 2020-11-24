import cv2
import numpy as np
from matplotlib import pyplot as plt

img = cv2.imread('download.jpg',0)
edges = cv2.Canny(img,100,200)
print(edges)

num = len([edges[x][y] for x in range(0, len(edges)) for y in range(0, len(edges[0])) if edges[x][y] == 255])

print(num)

plt.imshow(edges,cmap = 'gray')
plt.title('Edge Image'), plt.xticks([]), plt.yticks([])

plt.show()