FROM python

COPY . ./app
WORKDIR /app/

RUN ["pip", "install", "cython"]
RUN ["pip", "install", "Pillow"]
RUN ["pip", "install", "Flask"]
RUN ["pip", "install", "flask-restful"]

RUN ["sh", "python_setup.sh"]


WORKDIR /app/

EXPOSE 5100/tcp
ENTRYPOINT ["sh", "python_recognizer.sh"]