// Pure display component — no interactivity needed for catalogue browsing
const StarRating = ({ rating = 0, reviewCount = null, size = 'sm' }) => {
  const fullStars = Math.floor(rating);
  const hasHalf = rating - fullStars >= 0.5;
  const emptyStars = 5 - fullStars - (hasHalf ? 1 : 0);
  const fontSize = size === 'sm' ? '0.85rem' : '1.1rem';

  return (
    <span className="d-inline-flex align-items-center gap-1" style={{ fontSize }}>
      <span style={{ color: '#f5a623', letterSpacing: '1px' }}>
        {'★'.repeat(fullStars)}
        {hasHalf ? '½' : ''}
        <span style={{ color: '#ccc' }}>{'★'.repeat(emptyStars)}</span>
      </span>
      {reviewCount !== null && (
        <span className="text-muted" style={{ fontSize: '0.78rem' }}>
          ({reviewCount})
        </span>
      )}
    </span>
  );
};

export default StarRating;