import { useEffect, useState, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import {
  Container, Row, Col, Card, Button, Spinner,
  Alert, Form, Badge, Table, Image,
} from 'react-bootstrap';
import { FiMapPin, FiPlus, FiCheck, FiArrowLeft } from 'react-icons/fi';
import { toast } from 'react-toastify';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import {
  selectCartItems,
  selectCartSubtotal,
  selectCartTotalItems,
  selectHasOutOfStockItems,
} from '../store/slices/cartSlice.js';
import { getAddressesApi, addAddressApi } from '../api/addressApi.js';
import { createCheckoutSessionApi,generateIdempotencyKey } from '../api/paymentsApi.js';

const PLACEHOLDER = 'https://placehold.co/60x60?text=?';

const EMPTY_ADDRESS_FORM = {
  fullName: '',
  phoneNumber: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  state: '',
  postalCode: '',
  country: 'India',
  addressType: 'Home',
  isDefault: false,
};

const CheckoutPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const items = useSelector(selectCartItems);
  const subtotal = useSelector(selectCartSubtotal);
  const totalItems = useSelector(selectCartTotalItems);
  const hasOutOfStock = useSelector(selectHasOutOfStockItems);

  const [addresses, setAddresses] = useState([]);
  const [selectedAddressId, setSelectedAddressId] = useState(null);
  const [loadingAddresses, setLoadingAddresses] = useState(true);
  const [showAddressForm, setShowAddressForm] = useState(false);
  const [addressForm, setAddressForm] = useState(EMPTY_ADDRESS_FORM);
  const [savingAddress, setSavingAddress] = useState(false);
  const [placingOrder, setPlacingOrder] = useState(false);

  const idempotencyKeyRef = useRef(generateIdempotencyKey());

  // Redirect if cart is empty or has out-of-stock items
  useEffect(() => {
    if (!items.length) {
      navigate('/cart');
    }
  }, [items, navigate]);

  useEffect(() => {
    const loadAddresses = async () => {
      try {
        const res = await getAddressesApi();
        setAddresses(res.data);
        // Pre-select default address
        const def = res.data.find((a) => a.isDefault) || res.data[0];
        if (def) setSelectedAddressId(def.id);
        // If no addresses, show the add form automatically
        if (res.data.length === 0) setShowAddressForm(true);
      } catch {
        toast.error('Failed to load addresses');
      } finally {
        setLoadingAddresses(false);
      }
    };
    loadAddresses();
  }, []);

  const handleAddressFormChange = (e) => {
    const { name, value, type, checked } = e.target;
    setAddressForm((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSaveAddress = async (e) => {
    e.preventDefault();
    setSavingAddress(true);
    try {
      const res = await addAddressApi(addressForm);
      const newAddr = res.data;
      setAddresses((prev) => [...prev, newAddr]);
      setSelectedAddressId(newAddr.id);
      setShowAddressForm(false);
      setAddressForm(EMPTY_ADDRESS_FORM);
      toast.success('Address saved');
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to save address');
    } finally {
      setSavingAddress(false);
    }
  };

  const handlePlaceOrder = async () => {
    if (!selectedAddressId && addresses.length > 0) {
      toast.warning('Please select a delivery address');
      return;
    }
    if (hasOutOfStock) {
      toast.warning('Remove out-of-stock items before placing order');
      return;
    }
    setPlacingOrder(true);
    try {
      // ✅ Pass the stable key — if this call fails and the user clicks again,
      // the exact same key is sent, so the backend returns the cached session
      // instead of creating a duplicate order + Stripe session.
      const res = await createCheckoutSessionApi(
        selectedAddressId || null,
        idempotencyKeyRef.current
      );
      const { sessionUrl } = res.data;
      window.location.href = sessionUrl;
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to create checkout session');
      // ✅ Do NOT regenerate the key on error — reusing the same key on the
      // next click is exactly what idempotency is designed for.
      setPlacingOrder(false);
    }
  };

  if (loadingAddresses) {
    return (
      <>
        <AppNavbar />
        <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
          <Spinner animation="border" variant="primary" />
        </div>
        <Footer />
      </>
    );
  }

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container fluid="xl" className="py-4 flex-grow-1">
        {/* Back to cart */}
        <Button
          variant="link"
          className="text-muted p-0 mb-3 d-flex align-items-center gap-1"
          onClick={() => navigate('/cart')}
        >
          <FiArrowLeft size={14} /> Back to Cart
        </Button>

        <h4 className="fw-bold mb-4">Checkout</h4>

        <Row className="g-4">
          {/* ── Left: Address Selection ──────────────────────────── */}
          <Col lg={7}>
            <Card className="border-0 shadow-sm mb-4">
              <Card.Header className="bg-white fw-bold d-flex align-items-center gap-2">
                <FiMapPin /> Delivery Address
              </Card.Header>
              <Card.Body>
                {addresses.length > 0 && (
                  <div className="mb-3">
                    {addresses.map((addr) => (
                      <div
                        key={addr.id}
                        className={`p-3 mb-2 rounded border cursor-pointer ${
                          selectedAddressId === addr.id
                            ? 'border-primary bg-primary bg-opacity-10'
                            : 'border-light'
                        }`}
                        style={{ cursor: 'pointer' }}
                        onClick={() => setSelectedAddressId(addr.id)}
                      >
                        <div className="d-flex justify-content-between align-items-start">
                          <div>
                            <div className="d-flex align-items-center gap-2 mb-1">
                              <strong>{addr.fullName}</strong>
                              <Badge bg="secondary" pill style={{ fontSize: '0.65rem' }}>
                                {addr.addressType}
                              </Badge>
                              {addr.isDefault && (
                                <Badge bg="success" pill style={{ fontSize: '0.65rem' }}>
                                  Default
                                </Badge>
                              )}
                            </div>
                            <div className="text-muted small">
                              {addr.addressLine1}
                              {addr.addressLine2 && `, ${addr.addressLine2}`}
                              <br />
                              {addr.city}, {addr.state} — {addr.postalCode}
                              <br />
                              {addr.country}
                            </div>
                            <div className="text-muted small mt-1">📞 {addr.phoneNumber}</div>
                          </div>
                          {selectedAddressId === addr.id && (
                            <FiCheck size={18} className="text-primary flex-shrink-0" />
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                )}

                {/* Add new address toggle */}
                {!showAddressForm ? (
                  <Button
                    variant="outline-primary"
                    size="sm"
                    onClick={() => setShowAddressForm(true)}
                    className="d-flex align-items-center gap-1"
                  >
                    <FiPlus size={14} /> Add New Address
                  </Button>
                ) : (
                  <div className="border rounded p-3 mt-2">
                    <h6 className="fw-bold mb-3">New Address</h6>
                    <Form onSubmit={handleSaveAddress}>
                      <Row className="g-2">
                        <Col sm={6}>
                          <Form.Group>
                            <Form.Label className="small">Full Name *</Form.Label>
                            <Form.Control
                              size="sm"
                              name="fullName"
                              value={addressForm.fullName}
                              onChange={handleAddressFormChange}
                              required
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={6}>
                          <Form.Group>
                            <Form.Label className="small">Phone Number *</Form.Label>
                            <Form.Control
                              size="sm"
                              name="phoneNumber"
                              value={addressForm.phoneNumber}
                              onChange={handleAddressFormChange}
                              required
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={12}>
                          <Form.Group>
                            <Form.Label className="small">Address Line 1 *</Form.Label>
                            <Form.Control
                              size="sm"
                              name="addressLine1"
                              value={addressForm.addressLine1}
                              onChange={handleAddressFormChange}
                              required
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={12}>
                          <Form.Group>
                            <Form.Label className="small">Address Line 2</Form.Label>
                            <Form.Control
                              size="sm"
                              name="addressLine2"
                              value={addressForm.addressLine2}
                              onChange={handleAddressFormChange}
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={6}>
                          <Form.Group>
                            <Form.Label className="small">City *</Form.Label>
                            <Form.Control
                              size="sm"
                              name="city"
                              value={addressForm.city}
                              onChange={handleAddressFormChange}
                              required
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={6}>
                          <Form.Group>
                            <Form.Label className="small">State *</Form.Label>
                            <Form.Control
                              size="sm"
                              name="state"
                              value={addressForm.state}
                              onChange={handleAddressFormChange}
                              required
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={6}>
                          <Form.Group>
                            <Form.Label className="small">Postal Code *</Form.Label>
                            <Form.Control
                              size="sm"
                              name="postalCode"
                              value={addressForm.postalCode}
                              onChange={handleAddressFormChange}
                              required
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={6}>
                          <Form.Group>
                            <Form.Label className="small">Country *</Form.Label>
                            <Form.Control
                              size="sm"
                              name="country"
                              value={addressForm.country}
                              onChange={handleAddressFormChange}
                              required
                            />
                          </Form.Group>
                        </Col>
                        <Col sm={6}>
                          <Form.Group>
                            <Form.Label className="small">Address Type</Form.Label>
                            <Form.Select
                              size="sm"
                              name="addressType"
                              value={addressForm.addressType}
                              onChange={handleAddressFormChange}
                            >
                              <option value="Home">Home</option>
                              <option value="Work">Work</option>
                              <option value="Other">Other</option>
                            </Form.Select>
                          </Form.Group>
                        </Col>
                        <Col sm={6} className="d-flex align-items-end">
                          <Form.Check
                            type="checkbox"
                            name="isDefault"
                            label="Set as default"
                            checked={addressForm.isDefault}
                            onChange={handleAddressFormChange}
                            className="small"
                          />
                        </Col>
                      </Row>
                      <div className="d-flex gap-2 mt-3">
                        <Button
                          type="submit"
                          variant="primary"
                          size="sm"
                          disabled={savingAddress}
                        >
                          {savingAddress ? <Spinner animation="border" size="sm" /> : 'Save Address'}
                        </Button>
                        <Button
                          type="button"
                          variant="outline-secondary"
                          size="sm"
                          onClick={() => {
                            setShowAddressForm(false);
                            setAddressForm(EMPTY_ADDRESS_FORM);
                          }}
                          disabled={savingAddress}
                        >
                          Cancel
                        </Button>
                      </div>
                    </Form>
                  </div>
                )}
              </Card.Body>
            </Card>
          </Col>

          {/* ── Right: Order Summary ─────────────────────────────── */}
          <Col lg={5}>
            <Card className="border-0 shadow-sm mb-3">
              <Card.Header className="bg-white fw-bold">
                Order Summary ({totalItems} item{totalItems !== 1 ? 's' : ''})
              </Card.Header>
              <Card.Body className="p-0">
                {/* Item list */}
                <div style={{ maxHeight: '260px', overflowY: 'auto' }}>
                  {items.map((item, idx) => (
                    <div
                      key={item.cartItemId}
                      className={`px-3 py-2 d-flex align-items-center gap-3 ${
                        idx < items.length - 1 ? 'border-bottom' : ''
                      }`}
                    >
                      <Image
                        src={item.mainImageUrl || PLACEHOLDER}
                        alt={item.productName}
                        rounded
                        style={{ width: '48px', height: '48px', objectFit: 'cover', flexShrink: 0 }}
                        onError={(e) => { e.target.src = PLACEHOLDER; }}
                      />
                      <div className="flex-grow-1 min-width-0">
                        <div
                          className="fw-semibold text-truncate small"
                          style={{ maxWidth: '180px' }}
                        >
                          {item.productName}
                        </div>
                        <div className="text-muted" style={{ fontSize: '0.75rem' }}>
                          Qty: {item.quantity}
                        </div>
                      </div>
                      <div className="fw-semibold small text-nowrap">
                        ₹{item.lineTotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                      </div>
                    </div>
                  ))}
                </div>

                {/* Totals */}
                <div className="px-3 py-3 border-top">
                  <Table borderless size="sm" className="mb-0">
                    <tbody>
                      <tr>
                        <td className="text-muted small">Subtotal</td>
                        <td className="text-end small fw-semibold">
                          ₹{subtotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                      <tr>
                        <td className="text-muted small">Shipping</td>
                        <td className="text-end text-success small">Calculated by Stripe</td>
                      </tr>
                      <tr>
                        <td colSpan={2}><hr className="my-1" /></td>
                      </tr>
                      <tr>
                        <td className="fw-bold">Total (excl. shipping)</td>
                        <td className="text-end fw-bold text-primary">
                          ₹{subtotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    </tbody>
                  </Table>
                </div>
              </Card.Body>
            </Card>

            {hasOutOfStock && (
              <Alert variant="warning" className="py-2 small">
                ⚠️ Your cart has out-of-stock items. Please go back and remove them.
              </Alert>
            )}

            <div className="d-grid">
              <Button
                variant="success"
                size="lg"
                onClick={handlePlaceOrder}
                disabled={
                  placingOrder ||
                  hasOutOfStock ||
                  !items.length ||
                  (addresses.length > 0 && !selectedAddressId)
                }
                className="d-flex align-items-center justify-content-center gap-2"
              >
                {placingOrder ? (
                  <><Spinner animation="border" size="sm" /> Redirecting to Payment…</>
                ) : (
                  'Proceed to Payment'
                )}
              </Button>
              <small className="text-muted text-center mt-2" style={{ fontSize: '0.75rem' }}>
                🔒 Secured by Stripe. You'll be redirected to complete payment.
              </small>
            </div>
          </Col>
        </Row>
      </Container>

      <Footer />
    </div>
  );
};

export default CheckoutPage;