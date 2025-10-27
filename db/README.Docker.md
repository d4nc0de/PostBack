# Readme para DB en docker
- Se requiere los datos de autenticación en el .env
- El docker-compose incluye el contenedor neo4j-db y un contenedor auxiliar utilizado para incializar la base de datos
- Levantar los contenedores con `docker-compose up -d`
- Utilizar los puertos :7474 para acceso web y :7687 para acceso con Bolt
## Modelos de nodos y relaciones
Modelo de nodos esperados:
```Cypher
    (ux:USUARIO { idu: String[PK], nombre: String})
    (px:POST { idp: String[PK], contenido: String})
    (cx:COMENTARIO { idc: String(idp[FK] + consec)[PK], 
        fechorCom: DateTime, 
        likeNotLike: int,
        fechorAut: DateTime,
        contenidoCom: String})
```

Modelo de relaciones esperadas:
```Cypher
    (ux:USUARIO)-[:PUBLICA]->(px:POST) //1...N
    (px:POST)-[:TIENE]->(cx:COMENTARIO) //1...N
    (ux:USUARIO)-[:HACE]->(cx:COMENTARIO) //1...N
    (ux:USUARIO)-[:AUTORIZA]->(cx:COMENTARIO) //1...N
```