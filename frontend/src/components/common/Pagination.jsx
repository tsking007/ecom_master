import { Pagination as BsPagination } from 'react-bootstrap';

// Renders at most 5 page buttons around the current page
const Pagination = ({ pageNumber, totalPages, onPageChange }) => {
  if (totalPages <= 1) return null;

  const delta = 2;
  const range = [];
  for (
    let i = Math.max(1, pageNumber - delta);
    i <= Math.min(totalPages, pageNumber + delta);
    i++
  ) {
    range.push(i);
  }

  return (
    <BsPagination className="justify-content-center flex-wrap mt-4">
      <BsPagination.First
        disabled={pageNumber === 1}
        onClick={() => onPageChange(1)}
      />
      <BsPagination.Prev
        disabled={pageNumber === 1}
        onClick={() => onPageChange(pageNumber - 1)}
      />

      {range[0] > 1 && (
        <>
          <BsPagination.Item onClick={() => onPageChange(1)}>1</BsPagination.Item>
          {range[0] > 2 && <BsPagination.Ellipsis disabled />}
        </>
      )}

      {range.map((p) => (
        <BsPagination.Item
          key={p}
          active={p === pageNumber}
          onClick={() => onPageChange(p)}
        >
          {p}
        </BsPagination.Item>
      ))}

      {range[range.length - 1] < totalPages && (
        <>
          {range[range.length - 1] < totalPages - 1 && (
            <BsPagination.Ellipsis disabled />
          )}
          <BsPagination.Item onClick={() => onPageChange(totalPages)}>
            {totalPages}
          </BsPagination.Item>
        </>
      )}

      <BsPagination.Next
        disabled={pageNumber === totalPages}
        onClick={() => onPageChange(pageNumber + 1)}
      />
      <BsPagination.Last
        disabled={pageNumber === totalPages}
        onClick={() => onPageChange(totalPages)}
      />
    </BsPagination>
  );
};

export default Pagination;