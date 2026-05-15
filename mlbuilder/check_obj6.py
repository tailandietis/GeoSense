import psycopg2

conn = psycopg2.connect(host='localhost', port=5432, dbname='pelengdb', user='peleng', password='peleng123')
cur = conn.cursor()

cur.execute("SELECT obj, n, x, y, z FROM peleng.geophon WHERE obj=6")
rows = cur.fetchall()
print(f"Geophones obj=6: {len(rows)}")
for r in rows:
    print(r)

cur.execute("SELECT obj, xmin, xmax, ymin, ymax, zmin, zmax FROM peleng.geometr WHERE obj=6")
rows = cur.fetchall()
print(f"\nGeometry obj=6: {len(rows)}")
for r in rows:
    print(r)

conn.close()
