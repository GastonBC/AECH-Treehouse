# pylint: disable=locally-disabled, no-value-for-parameter

import streamlit as st
import numpy as np
import math
import random
import utils

import matplotlib
import matplotlib.pyplot as plt
from matplotlib import collections  as mc

import scipy
from scipy.spatial import Delaunay # pylint: disable=locally-disabled, no-name-in-module


# TODO paint result triangles

st.title('AECH-Treehouse')
st.subheader("Output")

# Sidebar settings
b_plot_randize = st.sidebar.button("Generate random forest and plot")

# Connection to upload coordinates of actual trees.
# point_file = st.sidebar.file_uploader(label="JSON file with trees locations as 'tree1':[1.8, 0.2], etc",
#                                       type=['json'])

ti_angle = st.sidebar.number_input("Minimal angle")

ti_ammount = st.sidebar.number_input("Tree quantity",min_value=3, max_value=None, value=3, step=1)

area_ops = ["No constraint", "Small", "Medium", "Big", "Surprise me"]
sb_area = st.sidebar.selectbox("Deck area", area_ops)

b_simple_test = st.sidebar.button("Simple tests")

if b_plot_randize:

    # Generates a random array of points in a plane simulating a forest.
    points = np.random.uniform(low=0, high=100, size=(ti_ammount, 2))

    # Triangulate the points according to Delaunay. Mercedes is working on another
    # triangulation
    tri = Delaunay(points)

    # Plot the main tree deck forest
    plt.triplot(points[:,0], points[:,1], tri.simplices)
    plt.plot(points[:,0], points[:,1], 'o')
    st.pyplot()

    # Area filter
    areas = []

    for triangle_points in points[tri.simplices]:
        areas.append(utils.triangle_area(triangle_points))

    area_range = max(areas) - min(areas)
    min_area = min(areas)
    max_area = max(areas)
    low_mid = min_area + (area_range/3)
    high_mid = min_area + ((area_range/3)*2)
    
    no_range = (min_area, max_area)
    low_range = ( min_area, low_mid  )
    mid_range = ( low_mid, high_mid )
    high_range = ( high_mid, max_area )
    
    if sb_area == area_ops[0]:
        check_range = no_range

    elif sb_area == area_ops[1]:
        check_range = low_range

    elif sb_area == area_ops[2]:
        check_range = mid_range

    elif sb_area == area_ops[3]:
        check_range = high_range

    elif sb_area == area_ops[4]:
        check_range = random.choice([low_range, mid_range, high_range, no_range])


    clean_vertexes = []
    deck_coords = []

    for triangle_points in points[tri.simplices]:

        # Area check
        if check_range[0] < utils.triangle_area(triangle_points) < check_range[1]:
            
            tri_angles = utils.angle_degrees(triangle_points)

            A_angle = tri_angles[0]
            B_angle = tri_angles[1]
            C_angle = tri_angles[2]

            if A_angle <= ti_angle or B_angle <= ti_angle or C_angle <= ti_angle:
                continue

            else:
                # To plot the triangles again, we do it adding line by line to a list, and
                # then plotting that list
                Ax = triangle_points[0][0]
                Ay = triangle_points[0][1]
                Bx = triangle_points[1][0]
                By = triangle_points[1][1]
                Cx = triangle_points[2][0]
                Cy = triangle_points[2][1]

                clean_vertexes.append([(Ax, Ay), (Bx, By)])
                clean_vertexes.append([(Bx, By), (Cx, Cy)])
                clean_vertexes.append([(Cx, Cy), (Ax, Ay)])

                deck_coords.append(triangle_points)

    # Plot the cleaned decks (plots lines, not triangles :( )
    lc = mc.LineCollection(clean_vertexes, linewidths=2)
    fig, ax = plt.subplots()
    ax.add_collection(lc)

    plt.plot(points[:,0], points[:,1], 'o')

    st.pyplot()

if b_simple_test:
    raise NotImplementedError

    single_triangles = [ [[0, 0], [1, 2], [1, 0]],
                         [[1,1], [0, 1], [0.5, 0]] ]

    d_tris = []
    for t in single_triangles:
        tr = Delaunay(single_triangles)
        d_tris.append(tr)

    # Plot the main tree deck forest
    t1 = plt.fill( [single_triangles[0][0][0], single_triangles[0][1][0], single_triangles[0][2][0]],
                   [single_triangles[0][0][1], single_triangles[0][1][1], single_triangles[0][2][1]] )
    plt.gca().add_patch(t1)
    st.pyplot()