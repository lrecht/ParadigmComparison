from typing import *

X = TypeVar('X')
Y = TypeVar('Y')

class Option(Generic[X]):
    """
    An optional type which can either have a value or be None
    It has mutiple helper methods to make manipulation easier
    """
    
    def __init__(self, value: X):
        self.__value = value
        self.is_empty = value is None
        self.has_value = not self.is_empty


    @classmethod
    def empty(_):
        """A class method to create an empty Option"""
        return Option[X](None)


    def map(self, function: Callable[[X], Y]) -> 'Option[Y]':
        """
        Takes a function which maps an Option of one type to an Option of another type
        Will return an empty Option of the new type if the current Option has no value
        """
        if self.is_empty:
            return Option[Y].empty()
        else:
            res = function(self.__value)
            return Option[Y](res)


    def flatmap(self, function: Callable[[X], 'Option[Y]']) -> 'Option[Y]':
        """
        Works like map, however takes a function which returns an Option - Will return the Option from that method
        """
        if self.is_empty:
            return function(self.__value)
        else:
            return Option[Y].empty()


    def or_else(self, other_value: X) -> X:
        """Returns the current value if is has a value, otherwise it will return the 'other_value'"""
        return self.__value if self.has_value else other_value


    def if_present(self, function: Callable[[X], None]) -> None:
        """A method which will execute the provided function if the Option has a value"""
        if self.has_value:
            function(self.__value)


    def get(self):
        """
        Returns the contained value. Could be 'None'
        It is recommended to use the 'has_value' property before calling this method
        """
        return self.__value