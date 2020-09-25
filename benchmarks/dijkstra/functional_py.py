from collections import namedtuple
from functools import reduce

edges = [("a", "b", 7),  ("a", "c", 9),
         ("a", "f", 14), ("b", "c", 10),
         ("b", "d", 15), ("c", "d", 11),
         ("c", "f", 2),  ("d", "e", 6),
         ("e", "f", 9)]

def getNewMoves(edges, oldPositions, pos, cost):
    moves = list(filter(lambda move: move[0] == pos, edges))
    movesToNewNodes = list(filter(lambda move: filter(lambda oldPos: move[1] == oldPos[0], oldPositions),moves))

    return list(map(lambda move: (move[0], move[1], move[2] + cost), moves))

def move(legalMoves, paths, dest):
    nextMove = reduce(lambda minimum, newMove:
                minimum if minimum[1] == dest 
                else (newMove if newMove[2] < minimum[2] or newMove[1] == dest else minimum),legalMoves)

    if nextMove[1] == dest:
        return list(filter(lambda path: path[0] == nextMove[0],paths))[0][1] + [(nextMove[0],nextMove[1])]
    else:
        newPaths = paths + [(nextMove[1], list(filter(lambda path: path[0] == nextMove[0],paths))[0][1] + [(nextMove[0],nextMove[1])])]
        oldMoves = list(filter(lambda a: a != nextMove,legalMoves))
        newMoves = getNewMoves(edges,newPaths,nextMove[1],nextMove[2]) + oldMoves
        return move(newMoves, newPaths, dest)

def shortestPath(edges, start, dest):
    moves = getNewMoves(edges, [(start,[])], start, 0)
    return move(moves, [(start,[])], dest)

shortestPath(edges, "a", "e")
