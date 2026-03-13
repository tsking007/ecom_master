import { useEffect, useState, useCallback } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useSelector } from 'react-redux';
import {
  Container, Row, Col, Card, Button, Badge,
  Spinner, Alert, Nav, Tab, Table, Image, Form, Modal,
} from 'react-bootstrap';
import {
  FiPackage, FiMapPin, FiUser, FiPlus, FiEdit2,
  FiTrash2, FiCheck, FiArrowRight,
} from 'react-icons/fi';
import { toast } from 'react-toastify';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import { selectCurrentUser } from '../store/slices/authSlice.js';
import { getOrderDetailsApi, getUserOrdersApi } from '../api/ordersApi.js';
import {
  getAddressesApi,
  addAddressApi,
  updateAddressApi,
  deleteAddressApi,
  setDefaultAddressApi,
} from '../api/addressApi.js';

const PLACEHOLDER = 'https://placehold.co/48x48?text=?';

// ── Helpers ───────────────────────────────────────────────────────────────────

const TRACKING_STATUS_COLOR = {
  Pending: 'warning',
  Processing: 'info',
  Shipped: 'primary',
  Delivered: 'success',
  Cancelled: 'danger',
  Returned: 'secondary',
};

const PAYMENT_STATUS_COLOR = {
  Paid: 'success',
  Pending: 'warning',
  Failed: 'danger',
  Refunded: 'secondary',
};

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

// ── Order Detail Modal ────────────────────────────────────────────────────────

const OrderDetailModal = ({ orderId, show, onHide }) => {
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!show || !orderId) return;
    setLoading(true);
    setError(null);
    getOrderDetailsApi(orderId)
      .then((res) => setOrder(res.data))
      .catch(() => setError('Failed to load order details'))
      .finally(() => setLoading(false));
  }, [show, orderId]);

  return (
    <Modal show={show} onHide={onHide} size="lg" centered scrollable>
      <Modal.Header closeButton>
        <Modal.Title className="fs-6 fw-bold">
          {order ? `Order #${order.orderNumber}` : 'Order Details'}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {loading && (
          <div className="text-center py-4">
            <Spinner animation="border" variant="primary" />
          </div>
        )}
        {error && <Alert variant="danger">{error}</Alert>}
        {order && (
          <>
            {/* Status row */}
            <div className="d-flex gap-2 mb-3 flex-wrap">
              <Badge bg={PAYMENT_STATUS_COLOR[order.paymentStatus] || 'secondary'}>
                Payment: {order.paymentStatus}
              </Badge>
              <Badge bg={TRACKING_STATUS_COLOR[order.trackingStatus] || 'secondary'}>
                Status: {order.trackingStatus}
              </Badge>
              {order.estimatedDeliveryDate && (
                <Badge bg="light" text="dark">
                  Est. Delivery: {new Date(order.estimatedDeliveryDate).toLocaleDateString('en-IN')}
                </Badge>
              )}
            </div>

            {/* Items */}
            {order.items.map((item, idx) => (
              <div
                key={idx}
                className={`d-flex align-items-center gap-3 py-2 ${
                  idx < order.items.length - 1 ? 'border-bottom' : ''
                }`}
              >
                <Image
                  src={item.productImageUrl || PLACEHOLDER}
                  alt={item.productName}
                  rounded
                  style={{ width: '48px', height: '48px', objectFit: 'cover', flexShrink: 0 }}
                  onError={(e) => { e.target.src = PLACEHOLDER; }}
                />
                <div className="flex-grow-1">
                  {item.productSlug ? (
                    <Link
                      to={`/products/${item.productSlug}`}
                      className="text-decoration-none text-dark fw-semibold small"
                    >
                      {item.productName}
                    </Link>
                  ) : (
                    <span className="fw-semibold small">{item.productName}</span>
                  )}
                  <div className="text-muted" style={{ fontSize: '0.75rem' }}>
                    Qty: {item.quantity} × ₹
                    {(item.discountedUnitPrice ?? item.unitPrice)?.toLocaleString('en-IN')}
                  </div>
                </div>
                <div className="fw-semibold small text-nowrap">
                  ₹{item.totalPrice?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                </div>
              </div>
            ))}

            {/* Totals */}
            <Table borderless size="sm" className="mt-3 mb-0">
              <tbody>
                <tr>
                  <td className="text-muted small">Subtotal</td>
                  <td className="text-end small">
                    ₹{order.subTotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                  </td>
                </tr>
                {order.discountAmount > 0 && (
                  <tr>
                    <td className="text-muted small">Discount</td>
                    <td className="text-end small text-success">
                      −₹{order.discountAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                    </td>
                  </tr>
                )}
                {order.shippingAmount > 0 && (
                  <tr>
                    <td className="text-muted small">Shipping</td>
                    <td className="text-end small">
                      ₹{order.shippingAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                    </td>
                  </tr>
                )}
                {order.taxAmount > 0 && (
                  <tr>
                    <td className="text-muted small">Tax</td>
                    <td className="text-end small">
                      ₹{order.taxAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                    </td>
                  </tr>
                )}
                <tr>
                  <td colSpan={2}><hr className="my-1" /></td>
                </tr>
                <tr>
                  <td className="fw-bold">Total</td>
                  <td className="text-end fw-bold text-primary">
                    ₹{order.totalAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                  </td>
                </tr>
              </tbody>
            </Table>

            {/* Shipping address */}
            {order.shippingAddressSnapshot && (() => {
                let addr = null;
                try {
                  addr = typeof order.shippingAddressSnapshot === 'string'
                    ? JSON.parse(order.shippingAddressSnapshot)
                    : order.shippingAddressSnapshot;
                } catch {
                  return (
                    <div className="mt-3 border-top pt-3">
                      <div className="text-muted small fw-semibold mb-1">Shipped To:</div>
                      <div className="text-muted small">{order.shippingAddressSnapshot}</div>
                    </div>
                  );
                }
                return (
                  <div className="mt-3 border-top pt-3">
                    <div className="text-muted small fw-semibold mb-1">Shipped To:</div>
                    <div className="text-muted small">
                      <div>{addr.FullName} {addr.PhoneNumber && `· ${addr.PhoneNumber}`}</div>
                      <div>{addr.AddressLine1}{addr.AddressLine2 && `, ${addr.AddressLine2}`}</div>
                      <div>{addr.City}, {addr.State} — {addr.PostalCode}</div>
                      <div>{addr.Country}</div>
                    </div>
                  </div>
                );
              })()}
          </>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" size="sm" onClick={onHide}>Close</Button>
      </Modal.Footer>
    </Modal>
  );
};

// ── Address Form Modal ────────────────────────────────────────────────────────

const AddressFormModal = ({ show, onHide, initial, onSaved }) => {
  const [form, setForm] = useState(initial || EMPTY_ADDRESS_FORM);
  const [saving, setSaving] = useState(false);

  // Sync when initial changes (edit mode)
  useEffect(() => {
    setForm(initial || EMPTY_ADDRESS_FORM);
  }, [initial, show]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      let res;
      if (initial?.id) {
        res = await updateAddressApi(initial.id, form);
      } else {
        res = await addAddressApi(form);
      }
      toast.success(initial?.id ? 'Address updated' : 'Address added');
      onSaved(res.data, !!initial?.id);
      onHide();
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to save address');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title className="fs-6 fw-bold">
          {initial?.id ? 'Edit Address' : 'Add New Address'}
        </Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body>
          <Row className="g-2">
            <Col sm={6}>
              <Form.Group>
                <Form.Label className="small">Full Name *</Form.Label>
                <Form.Control size="sm" name="fullName" value={form.fullName} onChange={handleChange} required />
              </Form.Group>
            </Col>
            <Col sm={6}>
              <Form.Group>
                <Form.Label className="small">Phone *</Form.Label>
                <Form.Control size="sm" name="phoneNumber" value={form.phoneNumber} onChange={handleChange} required />
              </Form.Group>
            </Col>
            <Col sm={12}>
              <Form.Group>
                <Form.Label className="small">Address Line 1 *</Form.Label>
                <Form.Control size="sm" name="addressLine1" value={form.addressLine1} onChange={handleChange} required />
              </Form.Group>
            </Col>
            <Col sm={12}>
              <Form.Group>
                <Form.Label className="small">Address Line 2</Form.Label>
                <Form.Control size="sm" name="addressLine2" value={form.addressLine2 || ''} onChange={handleChange} />
              </Form.Group>
            </Col>
            <Col sm={6}>
              <Form.Group>
                <Form.Label className="small">City *</Form.Label>
                <Form.Control size="sm" name="city" value={form.city} onChange={handleChange} required />
              </Form.Group>
            </Col>
            <Col sm={6}>
              <Form.Group>
                <Form.Label className="small">State *</Form.Label>
                <Form.Control size="sm" name="state" value={form.state} onChange={handleChange} required />
              </Form.Group>
            </Col>
            <Col sm={6}>
              <Form.Group>
                <Form.Label className="small">Postal Code *</Form.Label>
                <Form.Control size="sm" name="postalCode" value={form.postalCode} onChange={handleChange} required />
              </Form.Group>
            </Col>
            <Col sm={6}>
              <Form.Group>
                <Form.Label className="small">Country *</Form.Label>
                <Form.Control size="sm" name="country" value={form.country} onChange={handleChange} required />
              </Form.Group>
            </Col>
            <Col sm={6}>
              <Form.Group>
                <Form.Label className="small">Type</Form.Label>
                <Form.Select size="sm" name="addressType" value={form.addressType} onChange={handleChange}>
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
                checked={form.isDefault}
                onChange={handleChange}
                className="small"
              />
            </Col>
          </Row>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" size="sm" onClick={onHide} disabled={saving}>Cancel</Button>
          <Button type="submit" variant="primary" size="sm" disabled={saving}>
            {saving ? <Spinner animation="border" size="sm" /> : 'Save'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
};

// ── Orders Tab ────────────────────────────────────────────────────────────────

const OrdersTab = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrderId, setSelectedOrderId] = useState(null);
  const [showModal, setShowModal] = useState(false);

  useEffect(() => {
    getUserOrdersApi()
      .then((res) => setOrders(res.data))
      .catch(() => toast.error('Failed to load orders'))
      .finally(() => setLoading(false));
  }, []);

  const openOrder = (orderId) => {
    setSelectedOrderId(orderId);
    setShowModal(true);
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    );
  }

  if (!orders.length) {
    return (
      <div className="text-center py-5">
        <FiPackage size={48} className="text-muted mb-3" />
        <h6 className="text-muted">No orders yet</h6>
        <p className="text-muted small">Your completed orders will appear here.</p>
        <Button variant="primary" size="sm" as={Link} to="/products">
          Start Shopping <FiArrowRight className="ms-1" />
        </Button>
      </div>
    );
  }

  return (
    <>
      <div className="d-flex flex-column gap-3">
        {orders.map((order) => (
          <Card key={order.orderId} className="border-0 shadow-sm">
            <Card.Body className="p-3">
              <div className="d-flex justify-content-between align-items-start flex-wrap gap-2">
                <div>
                  <div className="fw-bold small">Order #{order.orderNumber}</div>
                  <div className="text-muted" style={{ fontSize: '0.75rem' }}>
                    {new Date(order.createdAt).toLocaleDateString('en-IN', {
                      day: 'numeric', month: 'short', year: 'numeric',
                    })}
                  </div>
                  <div className="d-flex gap-2 mt-1 flex-wrap">
                    <Badge
                      bg={PAYMENT_STATUS_COLOR[order.paymentStatus] || 'secondary'}
                      style={{ fontSize: '0.65rem' }}
                    >
                      {order.paymentStatus}
                    </Badge>
                    <Badge
                      bg={TRACKING_STATUS_COLOR[order.trackingStatus] || 'secondary'}
                      style={{ fontSize: '0.65rem' }}
                    >
                      {order.trackingStatus}
                    </Badge>
                  </div>
                </div>
                <div className="text-end">
                  <div className="fw-bold text-primary">
                    ₹{order.totalAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                  </div>
                  <div className="text-muted" style={{ fontSize: '0.72rem' }}>
                    {order.items.length} item{order.items.length !== 1 ? 's' : ''}
                  </div>
                  <Button
                    variant="outline-primary"
                    size="sm"
                    className="mt-2"
                    style={{ fontSize: '0.75rem' }}
                    onClick={() => openOrder(order.orderId)}
                  >
                    View Details
                  </Button>
                </div>
              </div>

              <div className="d-flex gap-2 mt-2 flex-wrap">
                {order.items.slice(0, 4).map((item, idx) => (
                  <Image
                    key={idx}
                    src={item.productImageUrl || PLACEHOLDER}
                    alt={item.productName}
                    rounded
                    title={item.productName}
                    style={{ width: '40px', height: '40px', objectFit: 'cover' }}
                    onError={(e) => { e.target.src = PLACEHOLDER; }}
                  />
                ))}
                {order.items.length > 4 && (
                  <div
                    className="d-flex align-items-center justify-content-center bg-light rounded text-muted"
                    style={{ width: '40px', height: '40px', fontSize: '0.7rem' }}
                  >
                    +{order.items.length - 4}
                  </div>
                )}
              </div>
            </Card.Body>
          </Card>
        ))}
      </div>

      <OrderDetailModal
        orderId={selectedOrderId}
        show={showModal}
        onHide={() => setShowModal(false)}
      />
    </>
  );
};

// ── Addresses Tab ─────────────────────────────────────────────────────────────

const AddressesTab = () => {
  const [addresses, setAddresses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editAddress, setEditAddress] = useState(null);
  const [deletingId, setDeletingId] = useState(null);
  const [settingDefaultId, setSettingDefaultId] = useState(null);

  const loadAddresses = useCallback(async () => {
    try {
      const res = await getAddressesApi();
      setAddresses(res.data);
    } catch {
      toast.error('Failed to load addresses');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAddresses();
  }, [loadAddresses]);

  const handleSaved = (saved, isEdit) => {
    if (isEdit) {
      setAddresses((prev) => prev.map((a) => (a.id === saved.id ? saved : a)));
    } else {
      setAddresses((prev) => [...prev, saved]);
    }
  };

  const handleDelete = async (addressId) => {
    if (!window.confirm('Delete this address?')) return;
    setDeletingId(addressId);
    try {
      await deleteAddressApi(addressId);
      setAddresses((prev) => prev.filter((a) => a.id !== addressId));
      toast.info('Address deleted');
    } catch {
      toast.error('Failed to delete address');
    } finally {
      setDeletingId(null);
    }
  };

  const handleSetDefault = async (addressId) => {
    setSettingDefaultId(addressId);
    try {
      const res = await setDefaultAddressApi(addressId);
      // Update local state: mark new default, unmark others
      setAddresses((prev) =>
        prev.map((a) => ({ ...a, isDefault: a.id === addressId }))
      );
      toast.success('Default address updated');
    } catch {
      toast.error('Failed to set default address');
    } finally {
      setSettingDefaultId(null);
    }
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    );
  }

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <span className="text-muted small">{addresses.length} saved address{addresses.length !== 1 ? 'es' : ''}</span>
        <Button
          variant="primary"
          size="sm"
          onClick={() => { setEditAddress(null); setShowModal(true); }}
          className="d-flex align-items-center gap-1"
        >
          <FiPlus size={13} /> Add Address
        </Button>
      </div>

      {addresses.length === 0 ? (
        <div className="text-center py-4">
          <FiMapPin size={40} className="text-muted mb-2" />
          <p className="text-muted small">No addresses saved yet.</p>
        </div>
      ) : (
        <div className="d-flex flex-column gap-3">
          {addresses.map((addr) => (
            <Card key={addr.id} className={`border-0 shadow-sm ${addr.isDefault ? 'border-start border-4 border-success' : ''}`}>
              <Card.Body className="p-3">
                <div className="d-flex justify-content-between align-items-start flex-wrap gap-2">
                  <div>
                    <div className="d-flex align-items-center gap-2 mb-1">
                      <strong className="small">{addr.fullName}</strong>
                      <Badge bg="secondary" pill style={{ fontSize: '0.62rem' }}>{addr.addressType}</Badge>
                      {addr.isDefault && (
                        <Badge bg="success" pill style={{ fontSize: '0.62rem' }}>
                          <FiCheck size={9} className="me-1" />Default
                        </Badge>
                      )}
                    </div>
                    <div className="text-muted small">
                      {addr.addressLine1}
                      {addr.addressLine2 && `, ${addr.addressLine2}`}
                    </div>
                    <div className="text-muted small">
                      {addr.city}, {addr.state} — {addr.postalCode}
                    </div>
                    <div className="text-muted small">{addr.country}</div>
                    <div className="text-muted small mt-1">📞 {addr.phoneNumber}</div>
                  </div>
                  <div className="d-flex flex-column gap-1 align-items-end">
                    {!addr.isDefault && (
                      <Button
                        variant="outline-success"
                        size="sm"
                        style={{ fontSize: '0.72rem' }}
                        onClick={() => handleSetDefault(addr.id)}
                        disabled={settingDefaultId === addr.id}
                      >
                        {settingDefaultId === addr.id
                          ? <Spinner animation="border" size="sm" />
                          : 'Set Default'}
                      </Button>
                    )}
                    <Button
                      variant="outline-secondary"
                      size="sm"
                      style={{ fontSize: '0.72rem' }}
                      onClick={() => { setEditAddress(addr); setShowModal(true); }}
                    >
                      <FiEdit2 size={11} className="me-1" />Edit
                    </Button>
                    <Button
                      variant="outline-danger"
                      size="sm"
                      style={{ fontSize: '0.72rem' }}
                      onClick={() => handleDelete(addr.id)}
                      disabled={deletingId === addr.id}
                    >
                      {deletingId === addr.id
                        ? <Spinner animation="border" size="sm" />
                        : <><FiTrash2 size={11} className="me-1" />Delete</>}
                    </Button>
                  </div>
                </div>
              </Card.Body>
            </Card>
          ))}
        </div>
      )}

      <AddressFormModal
        show={showModal}
        onHide={() => setShowModal(false)}
        initial={editAddress}
        onSaved={handleSaved}
      />
    </>
  );
};

// ── Profile Page (main) ───────────────────────────────────────────────────────

const ProfilePage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const user = useSelector(selectCurrentUser);
  const navigate = useNavigate();

  // tab from URL query param, default to 'orders'
  const activeTab = searchParams.get('tab') || 'orders';

  const handleTabSelect = (tab) => {
    setSearchParams({ tab });
  };

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container fluid="xl" className="py-4 flex-grow-1">
        <Row className="g-4">
          {/* ── Sidebar ──────────────────────────────────────────── */}
          <Col lg={3}>
            <Card className="border-0 shadow-sm mb-3">
              <Card.Body className="text-center p-4">
                <div
                  className="rounded-circle bg-primary text-white d-inline-flex align-items-center justify-content-center mb-3"
                  style={{ width: '64px', height: '64px', fontSize: '1.5rem' }}
                >
                  {user?.firstName?.[0]?.toUpperCase() || <FiUser />}
                </div>
                <div className="fw-bold">{user?.firstName} {user?.lastName}</div>
                <div className="text-muted small">{user?.email}</div>
              </Card.Body>
            </Card>

            {/* Sidebar nav */}
            <Card className="border-0 shadow-sm">
              <Card.Body className="p-2">
                <Nav className="flex-column gap-1">
                  <Nav.Link
                    className={`rounded px-3 py-2 small d-flex align-items-center gap-2 ${
                      activeTab === 'orders' ? 'bg-primary text-white' : 'text-dark'
                    }`}
                    onClick={() => handleTabSelect('orders')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FiPackage size={15} /> My Orders
                  </Nav.Link>
                  <Nav.Link
                    className={`rounded px-3 py-2 small d-flex align-items-center gap-2 ${
                      activeTab === 'addresses' ? 'bg-primary text-white' : 'text-dark'
                    }`}
                    onClick={() => handleTabSelect('addresses')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FiMapPin size={15} /> My Addresses
                  </Nav.Link>
                </Nav>
              </Card.Body>
            </Card>
          </Col>

          {/* ── Main Content ─────────────────────────────────────── */}
          <Col lg={9}>
            <Card className="border-0 shadow-sm">
              <Card.Header className="bg-white fw-bold d-flex align-items-center gap-2">
                {activeTab === 'orders' && <><FiPackage /> My Orders</>}
                {activeTab === 'addresses' && <><FiMapPin /> My Addresses</>}
              </Card.Header>
              <Card.Body>
                {activeTab === 'orders' && <OrdersTab />}
                {activeTab === 'addresses' && <AddressesTab />}
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </Container>

      <Footer />
    </div>
  );
};

export default ProfilePage;