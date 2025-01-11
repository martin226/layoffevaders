import streamlit as st
from streamlit_extras.app_logo import add_logo


@st.cache_data
def logo():
    
    add_logo("cow.jpeg", height=450)