import math

def calculate_distance(Ax,Ay,Bx,By):  
     dist = math.sqrt((Bx - Ax)**2 + (By - Ay)**2)  
     return dist  

def triangle_area(tri):
    Ax, Ay, Bx, By, Cx, Cy = tri[0][0], tri[0][1], tri[1][0], tri[1][1], tri[2][0], tri[2][1]
    return abs(0.5 * (((Bx-Ax)*(Cy-Ay))-((Cx-Ax)*(By-Ay))))

def angle_degrees(tri):
    Ax, Ay, Bx, By, Cx, Cy = tri[0][0], tri[0][1], tri[1][0], tri[1][1], tri[2][0], tri[2][1]

    a = calculate_distance(Bx, By, Cx, Cy)
    b = calculate_distance(Cx, Cy, Ax, Ay)
    c = calculate_distance(Ax, Ay, Bx, By)

    # Get the angles
    # Trigonometria
    cos_A_angle = ( ((b**2) + (c**2) - (a**2)) / (2*b*c) )
    cos_B_angle = ( ((c**2) + (a**2) - (b**2)) / (2*c*a) )
    cos_C_angle = ( ((a**2) + (b**2) - (c**2)) / (2*a*b) )

    A_angle = math.degrees(math.acos(cos_A_angle))
    B_angle = math.degrees(math.acos(cos_B_angle))
    C_angle = math.degrees(math.acos(cos_C_angle))

    return (A_angle, B_angle, C_angle)