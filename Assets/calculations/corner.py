from math import *
r = 3.35365853
x, y = 9.595, 26.99
for i in [1,2,3,4,5,6,7]:
    theta = i * pi / 16
    print(f'({cos(theta)*(r+.2):.4f}, {sin(theta)*(r+.2):.4f})\n')
