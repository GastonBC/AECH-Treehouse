import streamlit as st
import matplotlib
import matplotlib.pyplot as plt
from matplotlib import collections  as mc
import numpy as np
import model
import math
import numpy as np
import scipy
from scipy.spatial import Delaunay

def calculate_distance(x1,y1,x2,y2):  
     dist = math.sqrt((x2 - x1)**2 + (y2 - y1)**2)  
     return dist  


st.title('AECH-Treehouse')
st.subheader("Output")

# Sidebar settings
b_plot_randize = st.sidebar.button("Generate random forest and plot")

# point_file = st.sidebar.file_uploader(label="JSON file with trees locations as 'tree1':[1.8, 0.2], etc",
#                                       type=['json'])

ti_angle = st.sidebar.number_input("Minimal angle")

ti_ammount = st.sidebar.number_input("Tree quantity",min_value=0, max_value=None, value=0, step=1)

area_ops = ["Surprise me", "Small", "Medium", "Big"]
sb_area = st.sidebar.selectbox("Deck area", area_ops)

b_simple_test = st.sidebar.button("Simple tests")

if b_plot_randize:
    points = np.random.rand(ti_ammount, 2)
    tri = Delaunay(points)

    plt.figure()
    # The main tree deck forest
    plt.triplot(points[:,0], points[:,1], tri.simplices)
    plt.plot(points[:,0], points[:,1], 'o')
    st.pyplot()


    clean_points = []
    clean_vertexes = []

    for triangle_points in points[tri.simplices]:
        Ax = triangle_points[0][0]
        Ay = triangle_points[0][1]

        Bx = triangle_points[1][0]
        By = triangle_points[1][1]

        Cx = triangle_points[2][0]
        Cy = triangle_points[2][1]

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

        if A_angle <= ti_angle or B_angle <= ti_angle or C_angle <= ti_angle:
            continue
        else:

            clean_vertexes.append([(Ax, Ay), (Bx, By)])
            clean_vertexes.append([(Bx, By), (Cx, Cy)])
            clean_vertexes.append([(Cx, Cy), (Ax, Ay)])

    # The cleaned decks
    lc = mc.LineCollection(clean_vertexes, linewidths=2)
    fig, ax = plt.subplots()
    ax.add_collection(lc)

    plt.plot(points[:,0], points[:,1], 'o')

    st.pyplot()




    
if b_simple_test:
    xy = np.asarray([
        [-0.101, 0.872], [-0.080, 0.883]])
    x = np.degrees(xy[:, 0])
    y = np.degrees(xy[:, 1])

    triangles = np.asarray([
        [67, 66,  1], [65,  2, 66], [33, 72, 38],
        [33, 38, 34], [37, 35, 38], [34, 38, 35], [35, 37, 36]])

    print(len(xy))
    print(len(triangles))

    fig2, ax2 = plt.subplots()
    ax2.set_aspect('equal')
    ax2.triplot(x, y, triangles, 'go-', lw=1.0)
    ax2.set_title('triplot of user-specified triangulation')
    ax2.set_xlabel('Longitude (degrees)')
    ax2.set_ylabel('Latitude (degrees)')
    st.pyplot()