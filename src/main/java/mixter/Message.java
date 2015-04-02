package mixter;

import java.util.*;
import java.util.function.Consumer;

class Message {
    private final DecisionProjection projection;

    public Message(List<Event> eventHistory) {
        projection = new DecisionProjection(eventHistory);
    }

    public static void publish(PublishMessage publishMessage, EventPublisher eventPublisher) {
        MessageId messageId = new MessageId();
        eventPublisher.publish(new MessagePublished(messageId, publishMessage.getMessage(), publishMessage.getAuthorId()));
    }

    public void republish(UserId userId, EventPublisher eventPublisher) {
        if (projection.publishers.contains(userId)) {
            return;
        }
        MessageRepublished event = new MessageRepublished(projection.getId(), userId);
        eventPublisher.publish(event);
        projection.apply(event);
    }

    public void delete(UserId authorId, EventPublisher eventPublisher) {
        eventPublisher.publish(new MessageDeleted(projection.getId()));
    }

    private class DecisionProjection {
        private MessageId id;
        private Map<Class,Consumer> appliers=new HashMap<>();

        public Set<UserId> publishers = new HashSet<>();

        public DecisionProjection(List<Event> eventHistory) {
            Consumer<MessagePublished> applyMessagePublished = this::apply;
            Consumer<MessageRepublished> applyMessageRepublished = this::apply;
            appliers.put(MessagePublished.class, applyMessagePublished);
            appliers.put(MessageRepublished.class, applyMessageRepublished);
            eventHistory.forEach(this::apply);
        }

        public MessageId getId() {
            return id;
        }

        @SuppressWarnings("unchecked")
        public void apply(Event event){
            Consumer consumer = appliers.get(event.getClass());
            consumer.accept(event);
        }

        private void apply(MessagePublished event) {
            id = event.getMessageId();
            publishers.add(event.getAuthorId());
        }

        private void apply(MessageRepublished event) {
            publishers.add(event.getUserId());
        }

    }

    public static class MessageId {
        private final String id;

        public MessageId() {
            this.id = UUID.randomUUID().toString();
        }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (o == null || getClass() != o.getClass()) return false;

            MessageId messageId = (MessageId) o;

            return id.equals(messageId.id);

        }

        @Override
        public int hashCode() {
            return id.hashCode();
        }

        @Override
        public String toString() {
            return id;
        }

        public String getId() {
            return id;
        }
    }
}
