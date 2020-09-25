from collections import namedtuple
from functools import reduce
from pyrsistent import pmap

edges = [("a", "b", 7),  ("a", "c", 9),
         ("a", "f", 14), ("b", "c", 10),
         ("b", "d", 15), ("c", "d", 11),
         ("c", "f", 2),  ("d", "e", 6),
         ("e", "f", 9)]


def getNewMoves(edges, backtrackMap, pos, cost):
    # Filters all edges that are either the current pos or have already been visited
    moves = list(filter(lambda move: move[0] == pos and 
                                     move[1] not in backtrackMap.keys(), 
                                     edges))

    # Adds the cost of taking an edge
    return list(map(lambda move: (move[0], move[1], move[2] + cost), moves))

def backtrack(backtrackMap, position, result):
    if position == '0':
        return result
    else:
        return backtrack(backtrackMap, backtrackMap[position], [position] + result)

def move(legalMoves, backtrackMap, dest):
    nextMove = reduce(lambda minimum, newMove:
                minimum if minimum[1] == dest 
                else (newMove if newMove[2] < minimum[2] or newMove[1] == dest else minimum),legalMoves)

    if nextMove[1] == dest:
        return backtrack(backtrackMap,nextMove[0],[nextMove[1]])
    else:
        newBacktrackMap = backtrackMap.update({nextMove[1]:nextMove[0]})
        oldMoves = list(filter(lambda a: a[1] != nextMove[1],legalMoves))
        newMoves = getNewMoves(edges,newBacktrackMap,nextMove[1],nextMove[2]) + oldMoves
        return move(newMoves, newBacktrackMap, dest)


def shortestPath(edges, start, dest):
    moves = getNewMoves(edges, pmap({'a':'0'}), start, 0)
    return move(moves, pmap({'a':'0'}), dest)

shortestPath(edges, "a", "e")
